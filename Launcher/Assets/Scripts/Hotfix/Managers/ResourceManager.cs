// Assets/Scripts/Hotfix/ResourceManager.cs
// 【最终根除编译错误版】修正了所有异步加载和C#语法兼容性问题

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ResourceManager
{
    private static ResourceManager _instance;
    public static ResourceManager Instance => _instance ?? (_instance = new ResourceManager());

    public enum LoadMode { Editor, AssetBundle }
    public LoadMode CurrentLoadMode { get; private set; }

    private Dictionary<string, AssetBundle> _abCache = new Dictionary<string, AssetBundle>();
    private Dictionary<string, string> _assetPathToABNameMap = new Dictionary<string, string>();
    private Dictionary<string, string> _resNameToPathMap = new Dictionary<string, string>();
    // 【添加】下面这行代码
    private AssetBundleManifest _manifest = null;
    public void Init()
    {
#if UNITY_EDITOR
        CurrentLoadMode = (LoadMode)EditorPrefs.GetInt("EditorResourceMode", 0);
#else
        CurrentLoadMode = LoadMode.AssetBundle;
#endif
        if (CurrentLoadMode == LoadMode.AssetBundle)
        { // 【修改】调整加载顺序，第一步必须是加载总清单
            LoadAssetBundleManifest();
            LoadAssetMap();
            LoadResDB();
        }
    }
    private void LoadAssetBundleManifest()
    {
        // 总清单AB包的名字，就是我们在YEngineBuilder中设置的输出目录名
        string manifestName = "HotfixOutput";

        // 直接调用最底层的、不带依赖处理的加载方法来加载它自己
        AssetBundle manifestAB = LoadAssetBundleFromFile(manifestName);

        if (manifestAB != null)
        {
            // 从AB包中加载出 AssetBundleManifest 对象
            _manifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (_manifest == null)
            {
                Debug.LogError($"[ResourceManager] 从AB包 '{manifestName}' 中加载 AssetBundleManifest 对象失败！");
            }
        }
        else
        {
            Debug.LogError($"[ResourceManager] 核心清单AB包 '{manifestName}' 未找到! 依赖加载功能将失效。");
        }
    }
    private void LoadAssetMap()
    {
        TextAsset mapAsset = LoadAssetByFullPathInternal<TextAsset>("Configs/asset_map.json");
        if (mapAsset != null)
        {
            AssetMapWrapper wrapper = JsonUtility.FromJson<AssetMapWrapper>(mapAsset.text);
            if (wrapper != null && wrapper.AssetMapList != null)
            {
                _assetPathToABNameMap.Clear();
                foreach (var entry in wrapper.AssetMapList) _assetPathToABNameMap[entry.path] = entry.abName;
            }
        }
    }

    private void LoadResDB()
    {
        TextAsset dbAsset = LoadAssetByFullPathInternal<TextAsset>("Configs/res_db.json");
        if (dbAsset != null)
        {
            ResDBWrapper wrapper = JsonUtility.FromJson<ResDBWrapper>(dbAsset.text);
            if (wrapper != null && wrapper.ResMapList != null)
            {
                _resNameToPathMap.Clear();
                foreach (var entry in wrapper.ResMapList) _resNameToPathMap[entry.res] = entry.path;
            }
        }
    }

    // ==================== 公共 API ====================

    public T Load<T>(string resName) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (CurrentLoadMode == LoadMode.Editor)
        {
            return LoadAssetFromEditor<T>(resName);
        }
#endif
        string relativePath;
        if (_resNameToPathMap.TryGetValue(resName, out relativePath))
        {
            return LoadAssetByFullPathInternal<T>(relativePath);
        }
        Debug.LogError($"[ResourceManager] 在资源数据库(res_db)中找不到资源: '{resName}'");
        return null;
    }

    public async Task<T> LoadAsync<T>(string resName) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (CurrentLoadMode == LoadMode.Editor)
        {
            T asset = LoadAssetFromEditor<T>(resName);
            await Task.Yield();
            return asset;
        }
#endif
        string relativePath;
        if (_resNameToPathMap.TryGetValue(resName, out relativePath))
        {
            return await LoadAssetByFullPathInternalAsync<T>(relativePath);
        }
        Debug.LogError($"[ResourceManager] 在资源数据库(res_db)中找不到资源: '{resName}'");
        return null;
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        string sceneFullPath = $"Assets/GameRes_Hotfix/Scenes/{sceneName}.unity";
#if UNITY_EDITOR
        if (CurrentLoadMode == LoadMode.Editor)
        {
            EditorSceneManager.LoadScene(sceneFullPath, (LoadSceneMode)(OpenSceneMode)mode);
            return;
        }
#endif

        // 【核心修正】调用带依赖处理的加载方法
        string sceneABName;
        _assetPathToABNameMap.TryGetValue(sceneFullPath, out sceneABName);
        LoadAssetBundleWithDependencies(sceneABName); // <--- 原来这里调用的是不带依赖的 LoadAssetBundleFromFile

        SceneManager.LoadScene(sceneName, mode);
    }

    public async Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action<float> onProgress = null)
    {
        string sceneFullPath = $"Assets/GameRes_Hotfix/Scenes/{sceneName}.unity";
#if UNITY_EDITOR
        if (CurrentLoadMode == LoadMode.Editor)
        {
            AsyncOperation op = EditorSceneManager.LoadSceneAsync(sceneFullPath, (LoadSceneMode)mode);
            while (!op.isDone)
            {
                onProgress?.Invoke(op.progress);
                await Task.Yield();
            }
            return;
        }
#endif
        // 【核心修正】调用带依赖处理的异步加载方法
        string sceneABName;
        _assetPathToABNameMap.TryGetValue(sceneFullPath, out sceneABName);
        await LoadAssetBundleWithDependenciesAsync(sceneABName); // <--- 关键修改

        AsyncOperation sceneOp = SceneManager.LoadSceneAsync(sceneName, mode);
        while (!sceneOp.isDone)
        {
            onProgress?.Invoke(sceneOp.progress);
            await Task.Yield();
        }
    }

    public void UnloadAsset(string resName, bool unloadAllLoadedObjects)
    {
        string relativePath;
        if (_resNameToPathMap.TryGetValue(resName, out relativePath))
        {
            string fullPath = $"Assets/GameRes_Hotfix/{relativePath}";
            string abName;
            if (_assetPathToABNameMap.TryGetValue(fullPath, out abName))
            {
                AssetBundle cachedAB;
                abName = abName.ToLower();
                if (_abCache.TryGetValue(abName, out cachedAB))
                {
                    cachedAB.Unload(unloadAllLoadedObjects);
                    _abCache.Remove(abName);
                }
            }
        }
    }

    // ==================== 底层实现 ====================

#if UNITY_EDITOR
    private T LoadAssetFromEditor<T>(string resName) where T : UnityEngine.Object
    {
        if (_resNameToPathMap.Count == 0) ScanResDBInEditor();
        string relativePath;
        if (_resNameToPathMap.TryGetValue(resName, out relativePath))
        {
            return AssetDatabase.LoadAssetAtPath<T>($"Assets/GameRes_Hotfix/{relativePath}");
        }
        return null;
    }

    private void ScanResDBInEditor()
    {
        string resDBPath = "Assets/GameRes_Hotfix/Configs/res_db.json";
        if (File.Exists(resDBPath))
        {
            ResDBWrapper wrapper = JsonUtility.FromJson<ResDBWrapper>(File.ReadAllText(resDBPath));
            if (wrapper != null) foreach (var entry in wrapper.ResMapList) _resNameToPathMap[entry.res] = entry.path;
        }
    }
#endif

    private T LoadAssetByFullPathInternal<T>(string assetPathInRes) where T : UnityEngine.Object
    {
        string fullProjectPath = $"Assets/GameRes_Hotfix/{assetPathInRes}";
        string abName;
        _assetPathToABNameMap.TryGetValue(fullProjectPath, out abName);
        if (string.IsNullOrEmpty(abName))
        {
            if (assetPathInRes.StartsWith("Configs/")) abName = "configs.ab";
            else return null;
        }
        return LoadAssetFromAB<T>(abName, fullProjectPath);
    }

    private async Task<T> LoadAssetByFullPathInternalAsync<T>(string assetPathInRes) where T : UnityEngine.Object
    {
        string fullProjectPath = $"Assets/GameRes_Hotfix/{assetPathInRes}";
        string abName;
        _assetPathToABNameMap.TryGetValue(fullProjectPath, out abName);
        if (string.IsNullOrEmpty(abName))
        {
            if (assetPathInRes.StartsWith("Configs/")) abName = "configs.ab";
            else return null;
        }
        return await LoadAssetFromABAsync<T>(abName, fullProjectPath);
    }

    // 替换掉旧的 LoadAssetFromAB
    private T LoadAssetFromAB<T>(string abName, string assetPath) where T : UnityEngine.Object
    {
        // 【修改】现在调用带依赖处理的加载方法
        LoadAssetBundleWithDependencies(abName);

        // 从缓存中获取已加载的AB包
        AssetBundle targetAB;
        if (_abCache.TryGetValue(abName.ToLower(), out targetAB))
        {
            return targetAB.LoadAsset<T>(assetPath);
        }
        Debug.LogError($"[ResourceManager] AB包加载后，在缓存中依然找不到: {abName}");
        return null;
    }// 这是新的、带依赖处理的核心方法 (由旧的 LoadAssetBundleFromFile 改名并增强而来)
    private void LoadAssetBundleWithDependencies(string abName)
    {
        if (string.IsNullOrEmpty(abName)) return;
        abName = abName.ToLower();

        // 如果已加载，直接返回
        if (_abCache.ContainsKey(abName)) return;

        // 【核心】使用 _manifest 加载所有依赖项
        if (_manifest != null)
        {
            string[] dependencies = _manifest.GetAllDependencies(abName);
            foreach (var dep in dependencies)
            {
                // 递归加载依赖
                LoadAssetBundleWithDependencies(dep);
            }
        }
        else
        {
            Debug.LogWarning($"[ResourceManager] 因为核心清单未加载，无法处理 '{abName}' 的依赖关系。");
        }

        // 最后加载自己
        LoadAssetBundleFromFileInternal(abName);
    }

    // 这是新的、最底层的、只负责从文件加载的方法
    private AssetBundle LoadAssetBundleFromFileInternal(string abName)
    {
        if (string.IsNullOrEmpty(abName)) return null;
        abName = abName.ToLower();

        AssetBundle cachedAB;
        if (_abCache.TryGetValue(abName, out cachedAB)) return cachedAB;

        string finalPath = Path.Combine(Application.persistentDataPath, abName);
        if (!File.Exists(finalPath)) finalPath = Path.Combine(Application.streamingAssetsPath, abName);
        if (!File.Exists(finalPath)) return null;

        AssetBundle ab = AssetBundle.LoadFromFile(finalPath);
        if (ab != null) _abCache[abName] = ab;
        return ab;
    }

    private async Task<T> LoadAssetFromABAsync<T>(string abName, string assetPath) where T : UnityEngine.Object
    {
        // 【修改】调用带依赖处理的异步加载方法
        await LoadAssetBundleWithDependenciesAsync(abName);

        AssetBundle targetAB;
        if (_abCache.TryGetValue(abName.ToLower(), out targetAB))
        {
            AssetBundleRequest request = targetAB.LoadAssetAsync<T>(assetPath);
            // 使用 while 循环等待 AssetBundleRequest 完成
            while (!request.isDone)
            {
                await Task.Yield();
            }
            return request.asset as T;
        }

        Debug.LogError($"[ResourceManager] AB包异步加载后，在缓存中依然找不到: {abName}");
        return null;
    }
    private async Task LoadAssetBundleWithDependenciesAsync(string abName)
    {
        if (string.IsNullOrEmpty(abName)) return;
        abName = abName.ToLower();

        if (_abCache.ContainsKey(abName)) return;

        if (_manifest != null)
        {
            string[] dependencies = _manifest.GetAllDependencies(abName);
            // 【核心】使用 Task.WhenAll 来并行加载所有依赖项，效率更高
            List<Task> depTasks = new List<Task>();
            foreach (var dep in dependencies)
            {
                depTasks.Add(LoadAssetBundleWithDependenciesAsync(dep));
            }
            await Task.WhenAll(depTasks);
        }

        // 最后异步加载自己
        await LoadAssetBundleFromFileInternalAsync(abName);
    }

    private AssetBundle LoadAssetBundleFromFile(string abName)
    {
        if (string.IsNullOrEmpty(abName)) return null;
        abName = abName.ToLower();
        AssetBundle cachedAB;
        if (_abCache.TryGetValue(abName, out cachedAB)) return cachedAB;

        string finalPath = Path.Combine(Application.persistentDataPath, abName);
        if (!File.Exists(finalPath)) finalPath = Path.Combine(Application.streamingAssetsPath, abName);
        if (!File.Exists(finalPath)) return null;

        AssetBundle ab = AssetBundle.LoadFromFile(finalPath);
        if (ab != null) _abCache[abName] = ab;
        return ab;
    }


    private async Task<AssetBundle> LoadAssetBundleFromFileInternalAsync(string abName)
    {
        if (string.IsNullOrEmpty(abName)) return null;
        abName = abName.ToLower();

        AssetBundle cachedAB;
        if (_abCache.TryGetValue(abName, out cachedAB)) return cachedAB;

        string finalPath = Path.Combine(Application.persistentDataPath, abName);
        if (!File.Exists(finalPath)) finalPath = Path.Combine(Application.streamingAssetsPath, abName);
        if (!File.Exists(finalPath)) return null;

        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(finalPath);

        while (!request.isDone)
        {
            await Task.Yield();
        }

        AssetBundle ab = request.assetBundle;
        if (ab != null) _abCache[abName] = ab;
        return ab;
    }
}
