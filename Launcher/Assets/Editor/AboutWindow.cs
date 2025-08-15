using UnityEditor;
using UnityEngine;

public class AboutWindow : EditorWindow
{
    private Texture2D _logoTexture;
    private GUIStyle _headerStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _linkStyle;

    private const string LogoPath = "MyToolLogo"; 
    private const string WebsiteURL = "https://github.com/menghuan13251/YEngineLite"; 
    private const string QQGroupURL = "https://qm.qq.com/q/UVnaO2Nzi2"; 

   
    [MenuItem("YEngine/关于YEngine")]
    public static void ShowWindow()
    {
        // 创建并显示窗口。GetWindow<T>可以确保只存在一个实例。
        AboutWindow window = GetWindow<AboutWindow>(true, "YEngine使用帮助", true);
        window.minSize = new Vector2(350, 400); // 设置窗口最小尺寸
        window.maxSize = new Vector2(1000, 1000); // 设置窗口最大尺寸 (固定大小)
    }

    // 当窗口被启用时调用，用于初始化资源
    private void OnEnable()
    {
        // 从 "Assets/Editor/Resources" 文件夹加载Logo图片
        _logoTexture = Resources.Load<Texture2D>(LogoPath);
        if (_logoTexture == null)
        {
            Debug.LogWarning($"未能从 'Assets/Editor/Resources/{LogoPath}.png' 加载Logo图片。");
        }
    }

    // 初始化GUI样式的辅助方法
    private void InitializeStyles()
    {
        // 只在需要时创建样式，避免在OnGUI中重复创建
        if (_headerStyle == null)
        {
            _headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10)
            };
        }

        if (_bodyStyle == null)
        {
            _bodyStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true, // 自动换行
                fontSize = 12,
                margin = new RectOffset(15, 15, 5, 5)
            };
        }

        if (_linkStyle == null)
        {
            _linkStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 12,
                normal = { textColor = new Color(0.2f, 0.5f, 0.9f) }, // 蓝色，像超链接
                hover = { textColor = new Color(0.3f, 0.6f, 1.0f) } // 鼠标悬停时变亮
            };
        }
    }

    // 绘制窗口内容的核心方法
    private void OnGUI()
    {
        // 确保样式已经初始化
        InitializeStyles();

        // 垂直布局开始
        EditorGUILayout.BeginVertical();

        // 1. 绘制Logo图片 (自动缩放并保持比例)
        if (_logoTexture != null)
        {
            // --- 这是实现自动缩放的关键代码 ---

            // a. 获取当前布局区域的宽度，减去一些边距，让图片看起来不那么拥挤
            float availableWidth = EditorGUIUtility.currentViewWidth - 30f; // 左右各留15px边距

            // b. 根据图片原始宽高比，计算出在当前宽度下应该有的高度
            float aspectRatio = (float)_logoTexture.width / _logoTexture.height;
            float calculatedHeight = availableWidth / aspectRatio;

            // c. 使用 GUILayout.Box 来绘制图片，并应用计算出的宽高
            // 我们同时设置宽度和高度，并使用 ExpandWidth(false) 来确保它居中
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // 左侧空白，实现居中
            GUILayout.Box(_logoTexture, GUIStyle.none,
                GUILayout.Width(availableWidth),
                GUILayout.Height(calculatedHeight));
            GUILayout.FlexibleSpace(); // 右侧空白，实现居中
            GUILayout.EndHorizontal();
        }

        // 2. 绘制标题
        EditorGUILayout.LabelField("YEngine 傻瓜式热更引擎精简版", _headerStyle);

        // 分割线
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 3. 绘制介绍文字
        EditorGUILayout.LabelField(
            "感谢您使用本工具！这是一个轻量、高效的热更插件，旨在帮助您提高开发效率。\n使用方法：①YEngine/---【一键打包】---（打包成功弹出热更包文件夹）②复制HotfixOutput文件夹内的所有文件到服务器（示例：http://192.168.1.37:8088/Demo/Windows64）文件夹③打包你的exe运行④修改你的游戏项目再次重复①②然后运行exe成功热更。\n注意：标注颜色和说明的文件夹勿动！如需修改YEngine引擎系统请谨慎修改！！！\n如果您有任何问题或建议，欢迎通过以下方式联系我们。【访问项目主页 (GitHub)查看详细使用方法】",
            _bodyStyle
        );

        // 留出一些垂直空间
        GUILayout.Space(20);

        // 4. 绘制网页链接按钮
        DrawLinkButton("访问项目主页 (GitHub)", WebsiteURL);

        // 5. 绘制加入QQ群链接按钮
        DrawLinkButton("加入QQ交流群", QQGroupURL);

        // 留出一些垂直空间
        GUILayout.FlexibleSpace();

        // 绘制一个底部信息
        EditorGUILayout.LabelField("© 2024 YEngine", EditorStyles.centeredGreyMiniLabel);

        GUILayout.Space(5);

        // 垂直布局结束
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制一个看起来像超链接的按钮。
    /// </summary>
    /// <param name="label">按钮上显示的文字</param>
    /// <param name="url">要打开的链接</param>
    private void DrawLinkButton(string label, string url)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // 推到中间

        // 使用 GUILayout.Button 创建一个可点击的区域，但用我们自定义的链接样式来渲染它
        if (GUILayout.Button(label, _linkStyle))
        {
            // 当按钮被点击时，打开URL
            Application.OpenURL(url);
        }

        // 获取上一个控件（也就是我们的按钮）的矩形区域
        Rect buttonRect = GUILayoutUtility.GetLastRect();
        // 在按钮下方画一条下划线，让它更像超链接
        GUI.Box(new Rect(buttonRect.x, buttonRect.y + buttonRect.height - 1, buttonRect.width, 1), GUIContent.none, _linkStyle);

        GUILayout.FlexibleSpace(); // 推到中间
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10); // 按钮之间的间距
    }
}