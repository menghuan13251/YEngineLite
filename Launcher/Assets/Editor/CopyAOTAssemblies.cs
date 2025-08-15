using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class CopyAOTAssemblies : IPreprocessBuildWithReport
{
    // 定义私有字段和公共属性来缓存和获取FrameworkConfig
    private static FrameworkConfig _config;
    private static FrameworkConfig Config
    {
        get
        {
            if (_config == null)
            {
                // 自动在项目中查找FrameworkConfig类型的资产
                string[] guids = AssetDatabase.FindAssets("t:FrameworkConfig");
                if (guids.Length == 0)
                {
                    Debug.LogError("找不到 FrameworkConfig.asset 文件！请先通过菜单 '傻瓜式热更/创建框架配置' 来创建。");
                    return null;
                }
                // 通常项目中只有一个，我们取第一个
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _config = AssetDatabase.LoadAssetAtPath<FrameworkConfig>(path);
            }
            return _config;
        }
    }

    // IPreprocessBuildWithReport 接口确保在打包母包前自动执行
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        CopyFiles(report.summary.platform);
    }

    [MenuItem("YEngine/准备母包/拷贝AOT元数据到StreamingAssets")]
    public static void CopyFilesMenu()
    {
        CopyFiles(EditorUserBuildSettings.activeBuildTarget);
    }

    public static void CopyFiles(BuildTarget target)
    {
        // 首先确保能拿到配置
        if (Config == null)
        {
            Debug.LogError("[CopyAOTAssemblies] 因找不到FrameworkConfig配置而中断。");
            return;
        }

        // 源目录：从HybridCLR设置中获取
        string aotAssembliesSrcDir = Path.Combine(HybridCLR.Editor.Settings.HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());

        // 目标目录：StreamingAssets
        string aotAssembliesDstDir = Path.Combine(Application.streamingAssetsPath);

        if (!Directory.Exists(aotAssembliesSrcDir))
        {
            Debug.LogError($"[CopyAOTAssemblies] AOT元数据源目录不存在: '{aotAssembliesSrcDir}'. 请先执行 'HybridCLR/Generate/All'。");
            return;
        }

        if (!Directory.Exists(aotAssembliesDstDir))
        {
            Directory.CreateDirectory(aotAssembliesDstDir);
        }

       
        foreach (var dll in Config.AotMetaAssemblyFiles)
        {
            string srcDllPath = Path.Combine(aotAssembliesSrcDir, dll);
            if (File.Exists(srcDllPath))
            {
                File.Copy(srcDllPath, Path.Combine(aotAssembliesDstDir, dll), true);
            }
            else
            {
                // 这里使用Warning而不是Error，因为有时列表中配置的DLL可能在当前平台不存在
                Debug.LogWarning($"[CopyAOTAssemblies] 未在源目录中找到AOT元数据DLL: '{srcDllPath}'，已跳过。");
            }
        }

        Debug.Log($"✅ [CopyAOTAssemblies] AOT元数据DLL拷贝完成！");
        AssetDatabase.Refresh();
    }
}