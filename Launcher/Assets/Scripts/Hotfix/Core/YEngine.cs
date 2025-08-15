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

    // --- ������Դ���� API ---
    public static T LoadAsset<T>(string resName) where T : Object
    {
        return ResourceManager.Instance.Load<T>(resName);
    }

    // --- ���������Ƿ�����Դ���� API����������� ---
    public static Object LoadAsset(Type type, string resName)
    {
        // ͨ��������÷��͵� LoadAsset<T> ����
        MethodInfo method = typeof(YEngine).GetMethod("LoadAsset").MakeGenericMethod(type);
        return (Object)method.Invoke(null, new object[] { resName });
    }

    public static Task<T> LoadAssetAsync<T>(string resName) where T : Object
    {
        return ResourceManager.Instance.LoadAsync<T>(resName);
    }

    // --- �������� API ---
    public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        ResourceManager.Instance.LoadScene(sceneName, mode);
    }

    public static Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action<float> onProgress = null)
    {
        // �����ġ�ȷ��������õ��� ResourceManager ���첽�汾
        return ResourceManager.Instance.LoadSceneAsync(sceneName, mode, onProgress);
    }

    // --- ��Դж�� API ---
    public static void UnloadAsset(string resName, bool unloadAllLoadedObjects = false)
    {
        ResourceManager.Instance.UnloadAsset(resName, unloadAllLoadedObjects);
    }
    // ���������Ƿ��ͣ�ͨ���������·�����أ����ײ�ע��ʹ��
    public static Object LoadAssetByFullPath(Type type, string relativePath)
    {
        MethodInfo method = typeof(ResourceManager).GetMethod("LoadAssetByFullPathInternal").MakeGenericMethod(type);
        return (Object)method.Invoke(ResourceManager.Instance, new object[] { relativePath });
    }
}