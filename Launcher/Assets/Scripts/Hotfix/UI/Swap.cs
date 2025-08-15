// Assets/Scripts/Hotfix/Swap.cs
// �����մ����桿ֱ�Ӽ̳� MonoBehaviour

using UnityEngine;
using UnityEngine.UI;

// ������������ֱ�Ӽ̳� MonoBehaviour
public class Swap : MonoBehaviour
{
    // �������� public �� [SerializeField] �ֶΣ����ᱻ�Զ�ע��
    public Button myButton;
    public Text myText;
    public aaa aa;

    // ���������ɵ�ʹ�����б�׼��Unity�������ڷ���

    void Awake()
    {
        Debug.Log($"Swap.Awake() �ڶ��� '{gameObject.name}' �ϱ����ã�");
    }

    void Start()
    {
        Debug.Log("Swap.Start() �����ã���ʼ����ҵ���߼�...");

        if (myButton != null)
        {
            myButton.onClick.RemoveAllListeners();
            myButton.onClick.AddListener(OnMyButtonClick);
            Debug.Log("��ť�¼��󶨳ɹ���");
        }

        if (myText != null)
        {
            myText.text = "Hello, Hotfix World!";
        }
        var UI_Main = YEngine.LoadAsset<GameObject>("UI_Main");


            Instantiate(UI_Main, transform);
            Debug.Log("Prefab ʵ�����ɹ���");
        aa.Startaaa();
    }

    void Update()
    {
        // ���� Update �߼�
    }

    // �����¼�������
    public void OnMyButtonClick()
    {
        if (myText != null)
        {
            myText.text = "Button Clicked!";
        }
    }
}