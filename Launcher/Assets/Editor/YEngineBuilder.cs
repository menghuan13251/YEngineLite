// Editor/YEngineBuilder.cs
// 【终极完整版】一个按钮，幂等操作，无副作用，逻辑正确

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Text;
using System.Collections;

public class YEngineBuilder
{
    private const string HotfixAssemblyName = "Hotfix";
    private const string StubBaseClassName = "HotfixStub";
    private const string AOTStubDir = "Assets/Scripts/AOT/Stubs";
    private const string HotfixResRoot = "Assets/GameRes_Hotfix";
    private const string HotfixOutputDir = "HotfixOutput";
    private const string ConfigsABName = "configs.ab";
   
    // --- 【核心】唯一的菜单项 ---
    [MenuItem("YEngine/---【一键打包】---")]
    public static void UltimateBuild()
    {
        Debug.Log("================ 开始终极一键打包流程 ================");
        FrameworkConfig config = GetConfig();
        if (config == null) { Debug.LogError("打包中断：找不到 FrameworkConfig.asset 文件！"); return; }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogError("打包中断：请先保存当前场景的修改。");
            return;
        }
        string originalScenePath = SceneManager.GetActiveScene().path;

        try
        {
           // DeleteAllStubScriptFiles();
            // 步骤 1: 生成存根脚本 (先删除旧的，保证干净)
            var hotfixTypes = GenerateStubs();

            // 步骤 2: 执行幂等的、无损的注入操作
            if (hotfixTypes.Any())
            {
                InjectReferences(hotfixTypes);
            }
            else
            {
                Debug.LogWarning("项目中没有找到任何热更新脚本(MonoBehaviour)，跳过注入步骤。");
            }

            // 步骤 3: 打包资源
            BuildAllResources(config);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"打包过程中发生严重错误: {e}");
        }
        finally
        {
            // 无论成功与否，都恢复用户原来的工作场景
            if (!string.IsNullOrEmpty(originalScenePath) && File.Exists(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath);
            }
            Debug.Log("================ 终极一键打包流程结束 ================");
        }

        EditorUtility.RevealInFinder(HotfixOutputDir);
    }
   // [MenuItem("YEngine/工具/【危险】彻底删除所有存根脚本文件")]
    public static void DeleteAllStubScriptFiles()
    {
        Debug.Log($"--- 准备删除存根脚本目录: {AOTStubDir} ---");

        // 检查目录是否存在
        if (Directory.Exists(AOTStubDir))
        {
            try
            {
                // 递归删除整个文件夹及其所有内容
                Directory.Delete(AOTStubDir, true);

                // Unity 的资产数据库并不知道你直接删除了文件，
                // 我们还需要删除对应的 .meta 文件来避免错误。
                // 一个更安全、更符合Unity工作流的方式是使用 AssetDatabase.DeleteAsset。
                // 但因为我们删的是整个文件夹，直接删除后再刷新是最高效的。

                // 强制刷新资产数据库，让Unity编辑器能正确感知到文件夹已被删除
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                Debug.Log($"✅ 成功删除目录及其所有内容: {AOTStubDir}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"删除目录 '{AOTStubDir}' 时发生错误: {e.Message}");
            }
        }
        else
        {
            Debug.Log("目录不存在，无需删除。");
        }
    }
    [MenuItem("YEngine/工具/清理所有存根文件")]
    private static void ClearAllStubComponentsMenu()
    {
        Debug.Log("--- 开始清理所有存根组件 ---");
        DeleteAllStubScriptFiles();
        ClearAllStubComponents();
        AssetDatabase.SaveAssets();
        Debug.Log("✅ 清理完毕！");
    }

    // ==================== 核心实现 ====================

    private static List<System.Type> GenerateStubs()
    {
        Debug.Log("--- 步骤 1.1: 正在生成/更新存根(Stub)脚本 ---");
        if (!Directory.Exists(AOTStubDir)) Directory.CreateDirectory(AOTStubDir);

        var hotfixMonoTypes = GetHotfixTypes();
        var currentStubFiles = Directory.GetFiles(AOTStubDir, "*.cs").ToDictionary(p => Path.GetFileNameWithoutExtension(p), p => p);
        var hotfixTypeNames = hotfixMonoTypes.Select(t => t.Name).ToHashSet();

        foreach (var type in hotfixMonoTypes)
        {
            StringBuilder fieldDeclarations = new StringBuilder();
            HashSet<string> requiredNamespaces = new HashSet<string> { "UnityEngine" };
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                {
                    if (field.FieldType.Namespace != null) requiredNamespaces.Add(field.FieldType.Namespace);
                    fieldDeclarations.AppendLine($"    public {GetFriendlyTypeName(field.FieldType)} {field.Name};");
                }
            }
            StringBuilder usingStatements = new StringBuilder();
            foreach (var ns in requiredNamespaces.OrderBy(n => n)) usingStatements.AppendLine($"using {ns};");
            File.WriteAllText(Path.Combine(AOTStubDir, $"{type.Name}.cs"),
$@"// Auto-generated. Do not edit!
{usingStatements}
public class {type.Name} : {StubBaseClassName}
{{
{fieldDeclarations}
}}
");
            if (currentStubFiles.ContainsKey(type.Name)) currentStubFiles.Remove(type.Name);
        }

        foreach (var fileToRemove in currentStubFiles.Values)
        {
            AssetDatabase.DeleteAsset(fileToRemove);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        return hotfixMonoTypes;
    }

    private static void InjectReferences(List<System.Type> hotfixTypes)
    {
        Debug.Log("--- 步骤 1.2: 正在为场景和预制体注入/更新引用 ---");

        ClearAllStubComponents();

        string[] allPrefabPaths = Directory.GetFiles(HotfixResRoot, "*.prefab", SearchOption.AllDirectories);
        foreach (string path in allPrefabPaths)
        {
            using (var prefabScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                ProcessGameObject(prefabScope.prefabContentsRoot, hotfixTypes);
            }
        }
        string[] allScenePaths = Directory.GetFiles(HotfixResRoot, "*.unity", SearchOption.AllDirectories);
        foreach (string path in allScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bool sceneDirty = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (ProcessGameObject(go, hotfixTypes)) sceneDirty = true;
            }
            if (sceneDirty) EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
    }

    private static void ClearAllStubComponents()
    {
        string[] allPrefabPaths = Directory.GetFiles(HotfixResRoot, "*.prefab", SearchOption.AllDirectories);
        foreach (string path in allPrefabPaths)
        {
            using (var prefabScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                foreach (var stub in prefabScope.prefabContentsRoot.GetComponentsInChildren<HotfixStub>(true))
                    Object.DestroyImmediate(stub, true);
            }
        }
        string[] allScenePaths = Directory.GetFiles(HotfixResRoot, "*.unity", SearchOption.AllDirectories);
        foreach (string path in allScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bool sceneDirty = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                foreach (var stub in go.GetComponentsInChildren<HotfixStub>(true))
                {
                    Object.DestroyImmediate(stub, true);
                    sceneDirty = true;
                }
            }
            if (sceneDirty) EditorSceneManager.SaveScene(scene);
        }
    }

    private static bool ProcessGameObject(GameObject go, List<System.Type> hotfixTypes)
    {
        bool isDirty = false;
        foreach (var hotfixType in hotfixTypes)
        {
            Component[] hotfixComponents = go.GetComponentsInChildren(hotfixType, true);
            foreach (var hotfixComp in hotfixComponents)
            {
                GameObject targetGO = hotfixComp.gameObject;
                System.Type stubType = GetAOTStubType(hotfixType.Name);
                if (stubType == null) continue;

                HotfixStub stub = targetGO.AddComponent(stubType) as HotfixStub;
                stub.HotfixScriptFullName = hotfixType.FullName;
                stub.References = AnalyzeComponentReferences(hotfixComp);

                foreach (var reference in stub.References)
                {
                    FieldInfo stubField = stubType.GetField(reference.FieldName);
                    if (stubField != null) stubField.SetValue(stub, reference.ReferencedObject);
                }
                EditorUtility.SetDirty(stub);
                isDirty = true;
            }
        }
        return isDirty;
    }

    private static List<HotfixObjectReference> AnalyzeComponentReferences(Component component)
    {
        List<HotfixObjectReference> fieldRefs = new List<HotfixObjectReference>();
        System.Type type = component.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null) continue;
            object value = field.GetValue(component);
            if (value is UnityEngine.Object objRef && objRef != null)
                fieldRefs.Add(new HotfixObjectReference { FieldName = field.Name, ReferencedObject = objRef });
        }
        return fieldRefs;
    }

    private static void BuildAllResources(FrameworkConfig config)
    {
        ApplyLabelsAndGenerateMaps();
        CompileHotfixDLLs();
        BuildAllAssetBundles();
        GenerateVersionManifest();
        PrepareFirstPackRes(config);
    }

    private static void ApplyLabelsAndGenerateMaps()
    {
        foreach (string name in AssetDatabase.GetAllAssetBundleNames()) AssetDatabase.RemoveAssetBundleName(name, true);
        Dictionary<string, string> assetMap = new Dictionary<string, string>();
        var allFiles = new DirectoryInfo(HotfixResRoot).GetFiles("*", SearchOption.AllDirectories);
        foreach (var file in allFiles) SetLabelForFile(file, assetMap);

        string configDir = Path.Combine(HotfixResRoot, "Configs");
        Directory.CreateDirectory(configDir);
        string assetMapPath = Path.Combine(configDir, "asset_map.json");
        AssetMapWrapper mapWrapper = new AssetMapWrapper { AssetMapList = assetMap.Select(p => new AssetMapEntry { path = p.Key, abName = p.Value }).ToList() };
        File.WriteAllText(assetMapPath, JsonUtility.ToJson(mapWrapper, true));
        AssetDatabase.ImportAsset(assetMapPath);
        SetLabelForPath(assetMapPath, ConfigsABName, assetMap);

        Dictionary<string, string> resDB = new Dictionary<string, string>();
        foreach (var pair in assetMap)
        {
            string resName = Path.GetFileNameWithoutExtension(pair.Key);
            if (!resDB.ContainsKey(resName)) resDB[resName] = pair.Key.Substring(HotfixResRoot.Length + 1).Replace("\\", "/");
        }
        string resDBPath = Path.Combine(configDir, "res_db.json");
        ResDBWrapper wrapper = new ResDBWrapper { ResMapList = resDB.Select(p => new ResDBEntry { res = p.Key, path = p.Value }).ToList() };
        File.WriteAllText(resDBPath, JsonUtility.ToJson(wrapper, true));
        AssetDatabase.ImportAsset(resDBPath);
        SetLabelForPath(resDBPath, ConfigsABName, assetMap);
    }

    private static void SetLabelForFile(FileInfo file, Dictionary<string, string> assetMap)
    {
        if (file.Extension == ".meta" || file.Name.StartsWith(".")) return;
        string assetPath = file.FullName.Substring(Application.dataPath.Length - "Assets".Length).Replace("\\", "/");
        string dirPath = Path.GetDirectoryName(assetPath).Replace("\\", "/");
        string abName = (dirPath == HotfixResRoot) ? "general.ab" : dirPath.Substring(HotfixResRoot.Length + 1).Replace("/", "_").ToLower() + ".ab";
        SetLabelForPath(assetPath, abName, assetMap);
    }
    private static void SetLabelForPath(string assetPath, string abName, Dictionary<string, string> assetMap)
    {
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer != null) { importer.assetBundleName = abName.ToLower(); assetMap[assetPath] = abName.ToLower(); }
    }
    private static void CompileHotfixDLLs()
    {
        if (!Directory.Exists(HotfixOutputDir)) Directory.CreateDirectory(HotfixOutputDir);
        HybridCLR.Editor.Commands.CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);
        string hotfixDllSrcDir = Path.Combine(HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir, EditorUserBuildSettings.activeBuildTarget.ToString());
        File.Copy(Path.Combine(hotfixDllSrcDir, "Hotfix.dll"), Path.Combine(HotfixOutputDir, "Hotfix.dll"), true);
    }
    private static void BuildAllAssetBundles()
    {
        if (!Directory.Exists(HotfixOutputDir)) Directory.CreateDirectory(HotfixOutputDir);
        BuildPipeline.BuildAssetBundles(HotfixOutputDir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }
    private static void GenerateVersionManifest()
    {
        VersionManifestWrapper manifestWrapper = new VersionManifestWrapper();
        foreach (var file in Directory.GetFiles(HotfixOutputDir, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".manifest") || file.EndsWith(".meta") || Directory.Exists(file)) continue;
            string relativePath = file.Substring(HotfixOutputDir.Length + 1).Replace("\\", "/");
            manifestWrapper.FileList.Add(new VersionEntry { file = relativePath, manifest = new FileManifest { md5 = GetFileMD5(file), size = new FileInfo(file).Length } });
        }
        File.WriteAllText(Path.Combine(HotfixOutputDir, "version.json"), JsonUtility.ToJson(manifestWrapper, true));
    }
    private static void PrepareFirstPackRes(FrameworkConfig config)
    {
        string streamingAssets = Application.streamingAssetsPath;
        if (Directory.Exists(streamingAssets)) Directory.Delete(streamingAssets, true);
        Directory.CreateDirectory(streamingAssets);
        CopyAOTAssemblies.CopyFiles(EditorUserBuildSettings.activeBuildTarget);
        List<string> filesToCopy = new List<string> { "HotfixOutput" };
        if (config.IncludeHotfixDllInFirstPack) filesToCopy.Add("Hotfix.dll");
        filesToCopy.AddRange(config.FirstPackABNames);
        foreach (var fileName in filesToCopy.Distinct())
        {
            string srcPath = Path.Combine(HotfixOutputDir, fileName);
            if (File.Exists(srcPath)) File.Copy(srcPath, Path.Combine(streamingAssets, fileName), true);
        }
        AssetDatabase.Refresh();
    }

    private static List<System.Type> GetHotfixTypes()
    {
        var hotfixAsm = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == HotfixAssemblyName);
        return hotfixAsm?.GetTypes().Where(t => t.IsSubclassOf(typeof(MonoBehaviour))).ToList() ?? new List<System.Type>();
    }
    private static System.Type GetAOTStubType(string shortName)
    {
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            var type = asm.GetType(shortName);
            if (type != null && type.IsSubclassOf(typeof(HotfixStub))) return type;
        }
        return null;
    }
    private static string GetFriendlyTypeName(System.Type type)
    {
        if (type == null) return "null";
        if (type.IsGenericType) return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName).ToArray()) + ">";
        if (type.Namespace != null && (type.Namespace.StartsWith("UnityEngine") || type.Namespace.StartsWith("System"))) return type.FullName.Replace("+", ".");
        return type.Name;
    }
    private static string GetGameObjectPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }
    private static string GetFileMD5(string filePath)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            return System.BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
    }
    private static FrameworkConfig GetConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:FrameworkConfig");
        if (guids.Length == 0) { Debug.LogError("找不到 FrameworkConfig.asset 文件！"); return null; }
        return AssetDatabase.LoadAssetAtPath<FrameworkConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}