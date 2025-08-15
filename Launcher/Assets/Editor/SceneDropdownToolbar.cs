#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class SceneDropdownToolbar
{
    private static IMGUIContainer _imgui;

    private struct SceneInfo
    {
        public string FileName;
        public string Path;
    }
    private static List<SceneInfo> _scenes = new List<SceneInfo>();
    private static GUIContent _btnContent;

    static SceneDropdownToolbar()
    {
        EditorApplication.update -= MountToToolbar;
        EditorApplication.update += MountToToolbar;
    }

    private static void MountToToolbar()
    {
        // 如果已经挂载成功，则不再执行
        if (_imgui != null && _imgui.parent != null)
        {
            EditorApplication.update -= MountToToolbar;
            return;
        }

        var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        var toolbar = Resources.FindObjectsOfTypeAll(toolbarType).FirstOrDefault() as ScriptableObject;
        if (toolbar == null) return;

        var root = (VisualElement)toolbar.GetType().GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(toolbar);
        if (root == null) return;

        
        var playZone = root.Q("Step");
        if (playZone == null || playZone.parent == null) return;

        // --- 初始化我们的按钮（如果还没创建） ---
        if (_imgui == null)
        {
            // 为了避免重复添加，先尝试查找是否已存在
            _imgui = playZone.parent.Q<IMGUIContainer>("SceneSwitcherDropdown");
            if (_imgui == null)
            {
                _imgui = new IMGUIContainer(DrawGUI) { name = "SceneSwitcherDropdown" };
                _imgui.style.marginLeft = 20;
                _imgui.style.marginRight = 5;
                _imgui.style.height = 22;
                _imgui.style.alignSelf = Align.Center;
                _imgui.style.flexShrink = 0;

                
                // 1. 找到 "Play" 元素在它父容器中的索引位置
                int playZoneIndex = playZone.parent.IndexOf(playZone);

                // 2. 将我们的按钮插入到 "Play" 元素的后面（索引+1的位置）
                playZone.parent.Insert(playZoneIndex + 1, _imgui);
            }
        }

        // 确认挂载成功后，反注册update回调
        if (_imgui.parent != null)
        {
            // 初始化时刷新一次
            RefreshScenes();
            EditorApplication.projectChanged += RefreshScenes;
            AssemblyReloadEvents.afterAssemblyReload += RefreshScenes;
            EditorApplication.update -= MountToToolbar;
        }
    }

    private static void DrawGUI()
    {
        if (_btnContent == null)
        {
            _btnContent = new GUIContent(" 切换场景", EditorGUIUtility.IconContent("SceneAsset Icon")?.image)
            {
                tooltip =$"{SceneManager.GetActiveScene().name}\n\n①切换场景\n②找到场景位置\n③刷新场景列表"
            };
        }

        // 动态更新tooltip以显示当前场景名
       // _btnContent.tooltip = $"{SceneManager.GetActiveScene().name}\n\n切换场景\n按住 Alt/Shift 显示完整路径";

        using (new EditorGUI.DisabledScope(EditorApplication.isCompiling))
        {
            
            if (GUILayout.Button(_btnContent, EditorStyles.toolbarDropDown, GUILayout.Width(100), GUILayout.Height(22)))
            {
                ShowMenu();
            }
        }
    }

    

    private static void ShowMenu()
    {
        var menu = new GenericMenu();
        string currentPath = SceneManager.GetActiveScene().path;

        bool showPath = Event.current != null && (Event.current.alt || Event.current.shift);

        if (_scenes.Count == 0)
        {
            menu.AddDisabledItem(new GUIContent("（没有找到场景）"));
        }
        else
        {
            foreach (var s in _scenes.OrderBy(s => s.FileName, StringComparer.OrdinalIgnoreCase))
            {
                string label = showPath ? $"{s.FileName}    [{s.Path}]" : s.FileName;
                bool on = string.Equals(currentPath, s.Path, StringComparison.OrdinalIgnoreCase);
                menu.AddItem(new GUIContent(label), on, () => SwitchScene(s.Path));
            }
        }

        menu.AddSeparator("");
        menu.AddItem(new GUIContent("在 Project 中选中当前场景"), false, PingActiveSceneAsset);
        menu.AddItem(new GUIContent("刷新列表"), false, RefreshScenes);

        var r = GUILayoutUtility.GetLastRect();
        r.y += 22;
        menu.DropDown(r);
    }

    private static void RefreshScenes()
    {
        var guids = AssetDatabase.FindAssets("t:Scene");
        var list = new List<SceneInfo>(guids.Length);
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var fileName = Path.GetFileName(path);
            list.Add(new SceneInfo { FileName = fileName, Path = path });
        }
        _scenes = list;
        _imgui?.MarkDirtyRepaint();
    }

    private static void PingActiveSceneAsset()
    {
        var active = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(active.path))
        {
            EditorUtility.DisplayDialog("无法定位", "当前场景尚未保存到磁盘。", "确定");
            return;
        }

        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(active.path);
        if (asset != null)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }

    private static void SwitchScene(string path)
    {
        if (EditorApplication.isPlaying)
        {
            if (EditorUtility.DisplayDialog("退出播放并切换场景？", "需要先停止播放再切换。", "确定", "取消"))
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += () => OpenScene(path);
            }
            return;
        }
        OpenScene(path);
    }

    private static void OpenScene(string path)
    {
        if (string.Equals(SceneManager.GetActiveScene().path, path, StringComparison.OrdinalIgnoreCase)) return;

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            try
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("切换失败", e.Message, "确定");
            }
        }
    }
}
#endif