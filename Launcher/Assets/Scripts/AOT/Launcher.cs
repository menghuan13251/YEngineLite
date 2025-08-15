using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using HybridCLR;
using System.Linq;
using System.Text;

public class Launcher : MonoBehaviour
{
    [Header("UI (请从场景中拖拽)")]
    public Slider progressBar;
    public Text statusText;

    [Header("框架核心配置")]
    public FrameworkConfig config;

    [Header("调试选项")]
    [Tooltip("勾选后，每次启动都会清空本地缓存。发布时请务必取消勾选！")]
    public bool DevelopMode = true;

    private bool isExtracting = false;

    async void Start()
    {
        Application.runInBackground = true;
        if (config == null)
        {
            UpdateStatus("致命错误: 框架配置(FrameworkConfig)未在Inspector中设置！");
            return;
        }

        if (DevelopMode)
        {
            string path = Application.persistentDataPath;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                Debug.LogWarning($"[Launcher-DevMode] 已强制清空本地缓存目录: {path}");
            }
        }

        // 初始化UI
        UpdateProgress(0);
        UpdateStatus("正在初始化...");

        try
        {
            await ExtractFirstPackRes();
            await LoadMetadataForAOTAssemblies();
            await CheckAndUpdate();
            await LoadGame();
        }
        catch (Exception e)
        {
            UpdateStatus($"出现致命错误: {e.Message}");
            Debug.LogError(e);
        }
    }

    private async Task ExtractFirstPackRes()
    {
        string persistentDataPath = Application.persistentDataPath;

        // 【核心逻辑】只通过检查可写目录是否存在且非空来判断是否首次启动
        // DevelopMode下每次都会为空，所以也会执行解压
        if (!Directory.Exists(persistentDataPath) || !Directory.EnumerateFileSystemEntries(persistentDataPath).Any())
        {
            isExtracting = true;
            UpdateStatus("首次启动，正在解压基础资源...");
            string streamingAssetsPath = Application.streamingAssetsPath;

            // 如果StreamingAssets为空，说明是纯热更包，直接跳过
            if (!Directory.Exists(streamingAssetsPath) || !Directory.EnumerateFileSystemEntries(streamingAssetsPath).Any())
            {
                Debug.LogWarning("StreamingAssets 目录为空，跳过解压流程。");
                isExtracting = false;
                Directory.CreateDirectory(persistentDataPath); // 确保目录存在
                return;
            }

            // 完整拷贝 StreamingAssets 下的所有文件和文件夹到可写目录
            await CopyDirectoryAsync(streamingAssetsPath, persistentDataPath);
            Debug.Log("基础资源解压完成。");
            isExtracting = false;
        }
    }

    private async Task LoadMetadataForAOTAssemblies()
    {
        UpdateStatus("初始化运行时环境...");
        foreach (var aotDllName in config.AotMetaAssemblyFiles)
        {
            string dllPath = Path.Combine(Application.streamingAssetsPath, aotDllName);
            byte[] dllBytes = await ReadStreamingAssetBytesAsync(dllPath);
            if (dllBytes != null)
            {
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
                if (err != LoadImageErrorCode.OK)
                {
                    Debug.LogError($"加载AOT元数据DLL失败: {aotDllName}, 错误码: {err}");
                }
            }
            else
            {
                Debug.LogWarning($"未在StreamingAssets中找到AOT元数据DLL: {aotDllName}");
            }
        }
    }

    private async Task CheckAndUpdate()
    {
        while (isExtracting) await Task.Delay(100);
        UpdateStatus("正在连接服务器检查更新...");

        string manifestName = "version.json";
        Dictionary<string, FileManifest> serverManifestDict = new Dictionary<string, FileManifest>();
        Dictionary<string, FileManifest> localManifestDict = new Dictionary<string, FileManifest>();

        // Load Server Manifest
        try
        {
            using (UnityWebRequest www = UnityWebRequest.Get(Path.Combine(config.ServerUrl, manifestName)))
            {
                www.timeout = 5; // 设置5秒超时
                var op = await www.SendWebRequestAsTask(this);
                VersionManifestWrapper serverWrapper = JsonUtility.FromJson<VersionManifestWrapper>(op.downloadHandler.text);
                if (serverWrapper != null && serverWrapper.FileList != null)
                {
                    serverManifestDict = serverWrapper.FileList.ToDictionary(entry => entry.file, entry => entry.manifest);
                }
            }
        }
        catch (Exception e)
        {
            UpdateStatus("连接更新服务器失败，将以本地版本启动。");
            Debug.LogWarning($"获取服务器版本清单失败: {e.Message}");
            await Task.Delay(1000);
            return;
        }

        // Load Local Manifest (永远只从可写目录读取)
        string localManifestPath = Path.Combine(Application.persistentDataPath, manifestName);
        if (File.Exists(localManifestPath))
        {
            string json = await File.ReadAllTextAsync(localManifestPath);
            VersionManifestWrapper localWrapper = JsonUtility.FromJson<VersionManifestWrapper>(json);
            if (localWrapper != null && localWrapper.FileList != null)
            {
                localManifestDict = localWrapper.FileList.ToDictionary(entry => entry.file, entry => entry.manifest);
            }
        }

        // Compare and get download list
        List<string> downloadList = serverManifestDict
            .Where(serverFile => !localManifestDict.ContainsKey(serverFile.Key) || localManifestDict[serverFile.Key].md5 != serverFile.Value.md5)
            .Select(p => p.Key)
            .ToList();

        if (downloadList.Count == 0)
        {
            UpdateStatus("已是最新版本！");
            await Task.Delay(500);
            return;
        }

        // Start Download
        long totalDownloadSize = downloadList.Sum(file => serverManifestDict[file].size);
        long currentDownloadedSize = 0;

        for (int i = 0; i < downloadList.Count; i++)
        {
            string fileName = downloadList[i];

            using (UnityWebRequest www = UnityWebRequest.Get(Path.Combine(config.ServerUrl, fileName)))
            {
                var asyncOp = www.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    long downloadedBytes = (long)www.downloadedBytes;
                    long currentFileDownloaded = downloadedBytes > 0 ? downloadedBytes : (long)(asyncOp.progress * serverManifestDict[fileName].size);
                    float totalProgress = totalDownloadSize > 0 ? (float)(currentDownloadedSize + currentFileDownloaded) / totalDownloadSize : 0;
                    UpdateStatus($"下载中({i + 1}/{downloadList.Count}): {fileName} - {ToReadableSize(currentFileDownloaded)}/{ToReadableSize(serverManifestDict[fileName].size)}");
                    UpdateProgress(totalProgress);
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string filePath = Path.Combine(Application.persistentDataPath, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    await File.WriteAllBytesAsync(filePath, www.downloadHandler.data);
                    currentDownloadedSize += serverManifestDict[fileName].size;
                }
                else { throw new Exception($"下载文件失败: {fileName}"); }
            }
        }

        // Update Local Manifest
        string serverManifestJson = JsonUtility.ToJson(new VersionManifestWrapper { FileList = serverManifestDict.Select(p => new VersionEntry { file = p.Key, manifest = p.Value }).ToList() });
        await File.WriteAllTextAsync(localManifestPath, serverManifestJson);
    }

 

    private async Task LoadGame()
    {
        string hotfixDllPath = Path.Combine(Application.persistentDataPath, "Hotfix.dll");
        if (!File.Exists(hotfixDllPath))
        {
            hotfixDllPath = Path.Combine(Application.streamingAssetsPath, "Hotfix.dll");
        }
        if (!File.Exists(hotfixDllPath))
        {
            throw new Exception("热更新DLL (Hotfix.dll) 在任何位置都找不到！");
        }

        // 注意：这里为了简化，使用了同步读取。对于非常大的DLL，可以改回异步
        byte[] dllBytes = File.ReadAllBytes(hotfixDllPath);
        Assembly hotfixAssembly = Assembly.Load(dllBytes);

        
        System.Type entryType = hotfixAssembly.GetType("GameEntry");
        MethodInfo entryMethod = entryType?.GetMethod("StartGame", new System.Type[] { typeof(Assembly) });
        if (entryMethod != null)
        {
            // 调用并传入 hotfixAssembly 对象
            entryMethod.Invoke(null, new object[] { hotfixAssembly });
        }
        else
        {
            throw new Exception("在Hotfix.dll中找不到入口方法 GameEntry.StartGame(Assembly)!");
        }

        // 隐藏启动器UI
        if (this.gameObject.transform.parent != null)
        {
            this.gameObject.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
    // --- 辅助方法 ---

    private async Task CopyDirectoryAsync(string sourceDir, string destinationDir)
    {
        var allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < allFiles.Length; i++)
        {
            string filePath = allFiles[i];
            if (filePath.EndsWith(".meta")) continue;

            UpdateProgress((float)(i + 1) / allFiles.Length);

            string destPath = filePath.Replace(sourceDir, destinationDir);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath));

            byte[] bytes = await ReadStreamingAssetBytesAsync(filePath);
            if (bytes != null)
            {
                await File.WriteAllBytesAsync(destPath, bytes);
            }
        }
    }

    private async Task<byte[]> ReadStreamingAssetBytesAsync(string path)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (File.Exists(path)) return await File.ReadAllBytesAsync(path);
        return null;
#elif UNITY_ANDROID || UNITY_IOS
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            var op = await www.SendWebRequestAsTask(this);
            return op.result == UnityWebRequest.Result.Success ? op.downloadHandler.data : null;
        }
#endif
    }

    private void UpdateStatus(string text)
    {
        if (statusText != null) statusText.text = text;
        Debug.Log($"[Launcher] {text}");
    }

    private void UpdateProgress(float value)
    {
        if (progressBar != null) progressBar.value = value;
    }

    private string ToReadableSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{(double)bytes / 1024:F2} KB";
        return $"{(double)bytes / (1024 * 1024):F2} MB";
    }
}
