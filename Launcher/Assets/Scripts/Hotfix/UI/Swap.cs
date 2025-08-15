// Assets/Scripts/Hotfix/Swap.cs
// 【最终纯净版】直接继承 MonoBehaviour

using UnityEngine;
using UnityEngine.UI;

// 【核心修正】直接继承 MonoBehaviour
public class Swap : MonoBehaviour
{
    // 您的所有 public 或 [SerializeField] 字段，都会被自动注入
    public Button myButton;
    public Text myText;
    public aaa aa;

    // 您可以自由地使用所有标准的Unity生命周期方法

    void Awake()
    {
        Debug.Log($"Swap.Awake() 在对象 '{gameObject.name}' 上被调用！");
    }

    void Start()
    {
        Debug.Log("Swap.Start() 被调用！开始处理业务逻辑...");

        if (myButton != null)
        {
            myButton.onClick.RemoveAllListeners();
            myButton.onClick.AddListener(OnMyButtonClick);
            Debug.Log("按钮事件绑定成功！");
        }

        if (myText != null)
        {
            myText.text = "Hello, Hotfix World!";
        }
        var UI_Main = YEngine.LoadAsset<GameObject>("UI_Main");


            Instantiate(UI_Main, transform);
            Debug.Log("Prefab 实例化成功！");
        aa.Startaaa();
    }

    void Update()
    {
        // 您的 Update 逻辑
    }

    // 您的事件处理方法
    public void OnMyButtonClick()
    {
        if (myText != null)
        {
            myText.text = "Button Clicked!";
        }
    }
}