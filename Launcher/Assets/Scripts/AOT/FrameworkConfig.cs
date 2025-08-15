using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FrameworkConfig", menuName = "傻瓜式热更/创建框架配置", order = 0)]
public class FrameworkConfig : ScriptableObject
{
    [Header("服务器资源地址")]
    public string ServerUrl = "http://127.0.0.1:8080/hotfix/";

    // 【新增】一个开关，决定是否将 Hotfix.dll 打入首包
    [Header("首包设置")]
    [Tooltip("如果勾选，Hotfix.dll也会被拷贝到StreamingAssets，以支持无网络启动。")]
    public bool IncludeHotfixDllInFirstPack = true;

    [Tooltip("随母包发布的AB包列表 (首包资源)")]
    public List<string> FirstPackABNames = new List<string>
    {
        "configs.ab",
    };

    [Header("AOT补充元数据DLL列表")]
    public List<string> AotMetaAssemblyFiles = new List<string>
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
        "UnityEngine.CoreModule.dll",
    };
}