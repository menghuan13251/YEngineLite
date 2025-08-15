// 负责启动第一个场景
using UnityEngine;
using System.Reflection;

public class GameEntry
{
    private static bool isInitialized = false;

    public static void StartGame(Assembly hotfixAssembly)
    {
        if (isInitialized) return;
        isInitialized = true;

        Debug.Log("静态入口 GameEntry.StartGame() -> 准备激活热更新世界...");

        // 步骤 1: 激活 HotfixEntry，让它开始监听
        HotfixEntry.Start(hotfixAssembly);

        // 步骤 2: 初始化 YEngine (包含 ResourceManager)
        YEngine.Init();

        // 步骤 3: 决定并加载游戏的第一个场景
        Debug.Log("GameEntry 正在加载启动场景: MainScene...");
        YEngine.LoadScene("MainScene"); // 您可以在这里改成 "LoginScene" 或任何您想要的启动场景
    }
}