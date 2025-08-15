using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.Reflection;

public static class YEngine
{
    private static bool _isInitialized = false;
    public static void Init()
    {
        if (_isInitialized) return;
        ResourceManager.Instance.Init();
        _isInitialized = true;
    }

    // --- 泛型资源加载 API ---
    public static T LoadAsset<T>(string resName) where T : Object
    {
        return ResourceManager.Instance.Load<T>(resName);
    }

    // --- 【新增】非泛型资源加载 API，供反射调用 ---
    public static Object LoadAsset(Type type, string resName)
    {
        // 通过反射调用泛型的 LoadAsset<T> 方法
        MethodInfo method = typeof(YEngine).GetMethod("LoadAsset").MakeGenericMethod(type);
        return (Object)method.Invoke(null, new object[] { resName });
    }

    public static Task<T> LoadAssetAsync<T>(string resName) where T : Object
    {
        return ResourceManager.Instance.LoadAsync<T>(resName);
    }

    // --- 场景加载 API ---
    public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        ResourceManager.Instance.LoadScene(sceneName, mode);
    }

    public static Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action<float> onProgress = null)
    {
        // 【核心】确保这里调用的是 ResourceManager 的异步版本
        return ResourceManager.Instance.LoadSceneAsync(sceneName, mode, onProgress);
    }

    // --- 资源卸载 API ---
    public static void UnloadAsset(string resName, bool unloadAllLoadedObjects = false)
    {
        ResourceManager.Instance.UnloadAsset(resName, unloadAllLoadedObjects);
    }
    // 【新增】非泛型，通过完整相对路径加载，供底层注入使用
    public static Object LoadAssetByFullPath(Type type, string relativePath)
    {
        MethodInfo method = typeof(ResourceManager).GetMethod("LoadAssetByFullPathInternal").MakeGenericMethod(type);
        return (Object)method.Invoke(ResourceManager.Instance, new object[] { relativePath });
    }
}