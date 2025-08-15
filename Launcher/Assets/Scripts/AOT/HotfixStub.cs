// ���½ű������д���ű��Ļ���

using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// �����ȸ��½ű�����Ļ��ࡣ
/// ����Editor����Ϊ�ȸ��½ű��ġ��������������л��ֶ����á�
/// </summary>
[UnityEngine.Scripting.Preserve]
public abstract class HotfixStub : MonoBehaviour
{
    // ��¼����Ӧ���ȸ��½ű�����������
    [HideInInspector]
    public string HotfixScriptFullName;

    // ���л������ֶε�������Ϣ
    [HideInInspector]
    public List<HotfixObjectReference> References = new List<HotfixObjectReference>();
}

[Serializable]
public class HotfixObjectReference
{
    public string FieldName;
    public UnityEngine.Object ReferencedObject;
}