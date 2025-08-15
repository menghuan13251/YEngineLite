using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HotfixEntry : MonoBehaviour
{
    public static Assembly HotfixAssembly { get; private set; }
    private static HotfixEntry _instance;
    private static bool _isStarted = false;

    public static void Start(Assembly hotfixAssembly)
    {
        if (_isStarted) return;
        _isStarted = true;

        HotfixAssembly = hotfixAssembly;
        _instance = new GameObject("HotfixEntry").AddComponent<HotfixEntry>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(this.gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        YEngine.Init();
    }

    void Start()
    {
        YEngine.LoadScene("MainScene");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HotfixStub[] stubs = FindObjectsOfType<HotfixStub>();

        foreach (var stub in stubs)
        {
            GameObject targetGO = stub.gameObject;
            string hotfixClassName = stub.HotfixScriptFullName;

           
            if (string.IsNullOrEmpty(hotfixClassName))
            {
                Debug.LogWarning($"在对象 '{targetGO.name}' 上发现一个数据不完整的存根，已跳过。", targetGO);
                continue;
            }

            System.Type hotfixType = HotfixAssembly.GetType(hotfixClassName);
            if (hotfixType != null)
            {
                if (targetGO.GetComponent(hotfixType) != null) continue;

                Component realComponent = targetGO.AddComponent(hotfixType);
                InjectFieldReferences(realComponent, stub.References);
            }
        }

#if UNITY_EDITOR
        CleanUp(scene);
#endif
    }

    private void InjectFieldReferences(Component targetComponent, List<HotfixObjectReference> references)
    {
        if (references == null || references.Count == 0) return;
        System.Type type = targetComponent.GetType();
        foreach (var reference in references)
        {
            FieldInfo field = type.GetField(reference.FieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) continue;

            Object valueToSet = reference.ReferencedObject;

            if (valueToSet != null)
            {
                if (field.FieldType.IsAssignableFrom(valueToSet.GetType()))
                {
                    field.SetValue(targetComponent, valueToSet);
                }
                else if (valueToSet is Component comp && field.FieldType == typeof(GameObject))
                {
                    field.SetValue(targetComponent, comp.gameObject);
                }
                else if (valueToSet is GameObject go && typeof(Component).IsAssignableFrom(field.FieldType))
                {
                    Component targetComp = go.GetComponent(field.FieldType);
                    if (targetComp != null) field.SetValue(targetComponent, targetComp);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void CleanUp(Scene scene)
    {
        HotfixStub[] stubs = FindObjectsOfType<HotfixStub>();
        foreach (var stub in stubs) if (stub != null) Destroy(stub);
        foreach (var go in scene.GetRootGameObjects()) CleanMissingScriptsRecursively(go);
    }

    private void CleanMissingScriptsRecursively(GameObject go)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform) CleanMissingScriptsRecursively(child.gameObject);
    }
#endif
}

