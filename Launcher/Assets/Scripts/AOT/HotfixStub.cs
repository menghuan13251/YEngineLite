// 【新脚本】所有存根脚本的基类

using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 所有热更新脚本存根的基类。
/// 它在Editor中作为热更新脚本的“替身”，用于序列化字段引用。
/// </summary>
[UnityEngine.Scripting.Preserve]
public abstract class HotfixStub : MonoBehaviour
{
    // 记录它对应的热更新脚本的完整类名
    [HideInInspector]
    public string HotfixScriptFullName;

    // 序列化所有字段的引用信息
    [HideInInspector]
    public List<HotfixObjectReference> References = new List<HotfixObjectReference>();
}

[Serializable]
public class HotfixObjectReference
{
    public string FieldName;
    public UnityEngine.Object ReferencedObject;
}