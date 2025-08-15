using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public class FolderColorizer
{
    // --- 1. ��������������Ҫ����ɫ ---
    private static readonly Dictionary<string, Color> FolderColors = new Dictionary<string, Color>
    {
        { "Assets/Scripts/AOT",                 new Color(0.8f, 0.3f, 0.3f) },
        { "Assets/HybridCLRGenerate",                 new Color(0.8f, 0.3f, 0.3f) },
        { "Assets/StreamingAssets",                 new Color(0.8f, 0.3f, 0.3f) },
        { "Assets/HybridCLRData/Generated",     new Color(0.8f, 0.3f, 0.3f) },
        { "Assets/Scripts/Hotfix",              new Color(0.3f, 0.8f, 0.3f) },
        { "Assets/Scripts/Hotfix/Core",              new Color(0.8f, 0.3f, 0.3f) },
        { "Assets/Scripts/Hotfix/Managers",              new Color(0.8f, 0.3f, 0.3f) },
        { "Assets/GameRes_Hotfix",              new Color(0.3f, 0.8f, 0.3f) },
        { "Assets/Editor",                      new Color(0.7f, 0.4f, 0.9f) },
        { "Assets/Editor/Resources",                      new Color(0.8f, 0.3f, 0.3f) },
        { "Assets/Scenes",                      new Color(0.9f, 0.8f, 0.2f) },
        { "Assets/Scripts/AOT/Stubs",           new Color(0.6f, 0.6f, 0.6f) },
        { "Assets/GameRes_Hotfix/Configs",      new Color(0.7f, 0.4f, 0.9f) },
        { "Assets/Plugins",      new Color(0.7f, 0.4f, 0.9f) },
        { "Assets/Scripts",      new Color(0.7f, 0.4f, 0.9f) },
    };

    // --- 2. ��������������Ҫ���ļ���˵�� ---
    private static readonly Dictionary<string, string> FolderDescriptions = new Dictionary<string, string>
    {{ "Assets/GameRes_Hotfix/Configs",    "�����ļ�" },
    { "Assets/HybridCLRGenerate",     "��٢�ȸ��ļ�" },
    { "Assets/Scripts/Hotfix/Managers",     "�ȸ����ݹ���" },
    { "Assets/Scripts/Hotfix/Core",     "�ȸ��������" },
        { "Assets/Scripts/AOT", "AOT/����Ԫ���ݴ���" },
        { "Assets/Scripts/AOT/Stubs", "����ļ�" },
        { "Assets/Scripts/Hotfix", "�ȸ�ҵ���߼�" },
        { "Assets/GameRes_Hotfix", "��Ҫ��AB�����ȸ���Դ" },
        { "Assets/Editor", "�༭���ű�" },
        { "Assets/Scenes", "��Ϸ���/����������" },
        { "Assets/HybridCLRData/Generated", "HybridCLR�Զ������ļ�" },
        { "Assets/Plugins", "ԭ��������������" },
        { "Assets/StreamingAssets", "�װ����ÿ�" },
    };
    // ------------------------------------

    // ����˵���ı�����ʽ�������ظ�����
    private static GUIStyle _descriptionStyle;

    static FolderColorizer()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);

        // ���·����Ч�����ļ��У�������
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        // --- ����һ�������ļ�����ɫ ---
        if (FolderColors.TryGetValue(path, out Color color))
        {
            DrawFolderColor(selectionRect, color);
        }

        // --- ���ܶ������ļ��������Ҳ����˵�� ---
        if (FolderDescriptions.TryGetValue(path, out string description))
        {
            DrawFolderDescription(selectionRect, description);
        }
    }

    /// <summary>
    /// �����ļ�����ɫ����
    /// </summary>
    private static void DrawFolderColor(Rect rect, Color color)
    {
        Rect backgroundRect = rect;

        // ������ͼ (ͼ���)
        if (rect.height > 20f)
        {
            backgroundRect.width = backgroundRect.height;
        }
        // ������ͼ (�б�)
        else
        {
            backgroundRect.x += 16f; // �ճ�ͼ��λ��
            backgroundRect.width -= 16f;
        }

        // ���ư�͸������
        Color backgroundColor = color;
        backgroundColor.a = 0.3f;
        EditorGUI.DrawRect(backgroundRect, backgroundColor);

        // �����������
        Rect lineRect = new Rect(rect.x, rect.y, 3, rect.height);
        EditorGUI.DrawRect(lineRect, color);
    }

    /// <summary>
    /// ����ľ��������ڣ����һ���˵������
    /// </summary>
    private static void DrawFolderDescription(Rect rect, string description)
    {
        // ��ʼ����ʽ (ֻ�ڵ�һ��ʱ����)
        if (_descriptionStyle == null)
        {
            _descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight, // �Ҷ���
                fontStyle = FontStyle.Italic,
                normal = { textColor = Color.white }
            };
        }

        // �ڵ�����ͼ�£�Ϊ�˲��Ϳ��ܴ��ڵ��ļ���С�����͵���Ϣ�ص���������΢�����ƶ�
        if (rect.height <= 20f)
        {
            rect.x -= 70; // ����ƫ�ƣ����Ҳ��������Ϣ�����ռ�
        }

        // ����һ���ұ߾�
        rect.x -= 5;

        // ���ƴ����ŵ�˵������
        GUI.Label(rect, $"({description})", _descriptionStyle);
    }
}