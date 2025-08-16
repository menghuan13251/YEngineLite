![Image](https://github.com/user-attachments/assets/b894b9ab-0d31-4fec-a5b8-e969590fe206)
# YEngine - 一款一键式傻瓜操作的 Unity 热更新框架

![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blueviolet)
![License](https://img.shields.io/badge/License-MIT-green)
![Status](https://img.shields.io/badge/Status-Active-brightgreen)
[![Docs](https://img.shields.io/badge/Documentation-View%20Online-blue)](https://github.com/menghuan13251/YEngineLite/blob/main/%E9%A1%B9%E7%9B%AE%E6%90%AD%E5%BB%BA%E6%B5%81%E7%A8%8B.docx)
[![QQ群](https://img.shields.io/badge/加入-QQ群-blueviolet)](https://qm.qq.com/q/UVnaO2Nzi2)
[![B站视频](https://img.shields.io/badge/B站-视频-blueviolet)](https://www.bilibili.com/video/BV15hYvzREfU/?share_source=copy_web&vd_source=1a86cf8af853611f2fe5c667035553fb)

欢迎使用 YEngine！这是一款基于 [HybridCLR](https://hybridclr.doc.code-philosophy.com/) 的 Unity 热更新框架，旨在提供一个清晰、高效、易于上手的开发环境。无论你是初次接触热更新，还是经验丰富的老手，YEngine 都能帮助你快速构建可热更新的项目。

## ✨ 核心特性

- **🚀 高性能热更新**: 基于 `HybridCLR`，实现了全 C# 代码的热更新，性能卓越，接近原生 AOT 代码。
- **📁 结构清晰**: 目录结构经过精心设计，将 AOT 模块、热更模块、框架核心与游戏业务逻辑完全分离，职责分明。
- **一键式构建**: 内置强大的 `YEngineBuilder` 编辑器工具，将复杂的补充元数据、生成桥接函数、构建热更 DLL 和打包 AssetBundle 的流程集成为一键式操作。
- **约定优于配置**: 开发者只需将代码和资源放入指定目录，无需进行复杂的配置，即可享受热更新带来的便利。
- **易于扩展**: 框架核心代码与业务逻辑解耦，方便您根据项目需求进行二次开发和功能扩展。

## 🚀 快速上手 (Quick Start)

使用本框架进行开发非常简单，您只需要关心两个核心目录：

1.  **编写热更新代码**:
    将您所有的游戏业务逻辑 C# 脚本放入 `Assets/Scripts/Hotfix/` 目录下。例如，您可以创建 `GameLogic` 文件夹来组织您的代码。
    ```
    Assets/Scripts/Hotfix/GameLogic/
    ├── MyPlayerController.cs
    └── UIManager.cs
    ```

2.  **放置热更新资源**:
    将所有需要热更新的资源，如预制体(Prefabs)、场景(Scenes)、配置文件(Configs)、贴图(Textures)等，放入 `Assets/GameRes_Hotfix/` 目录下。
    ```
    Assets/GameRes_Hotfix/
    ├── Prefabs/
    │   └── Player.prefab
    └── Scenes/
        └── Level_01.unity
    ```

3.  **执行打包流程**:
    打开 Unity 编辑器，从顶部菜单栏选择 `YEngine/---【一键打包】---`，然后等待片刻热更资源文件夹HotfixOutput会自动打开，此时你只需要把HotfixOutput文件夹内的所有内容复制到你的服务器指定文件夹即可，例如：（http://192.168.1.37:8088/Demo/Windows64）。

4.  **热更流程**:
    使用unity打包发布成exe，然后修改你的项目，再次执行3.  **执行打包流程**:，再次打开你打包得exe就可以发现热更已完成！

 5.  **注意事项**:
     上述所有操作仅限于在此项目内进行，如果你要是把项目文件导入你的项目需要进行一次完整的流程操作，具体流程可查看【项目搭建流程.docx】

     
就是这么简单！框架会自动处理后续所有复杂的流程。

## 📂 目录结构详解

为了让您更好地理解框架的工作方式，以下是详细的目录结构说明。我们使用了 Emoji 来表示不同目录的用途和权限：

<img width="873" height="453" alt="Image" src="https://github.com/user-attachments/assets/c99bd767-d84a-497a-ad01-952250dc086b" />

-   🟢 **开发者工作区 (可自由修改/创建)**
-   🔴 **框架核心 (请勿修改)**
-   🟣 **核心/插件 (请勿随意修改)**
-   🟡 **核心场景 (请勿修改)**
-   ⚫️ **自动生成 (可按需清理)**

```
Assets/
├── 📂 Editor/
│   └── 📜 YEngineBuilder.cs      # 🔴核心构建工具，集成所有打包流程
│
├── 📂 Scenes/
│   └── 🟡 Main.unity             # 🔴主入口场景，负责启动热更流程 (请勿修改)
│
├── 📂 GameRes_Hotfix/             # 🟢 所有热更新资源的大本营 (开发者主要工作区)
│   ├── 📂 Scenes/                 #   你的游戏场景
│   ├── 📂 Prefabs/                #   你的预制体
│   ├── 📂 Configs/                #   🔴打包构建的配置文件夹
│   └── 📂 ... (其他)             #   你可以自由创建子目录来组织资源
│
├── 📂 HybridCLRGenerate/          # 🔴 HybridCLR 自动生成文件目录 (请勿修改)
│   └── 📜 link.xml              #   防止代码被 IL2CPP 裁剪的配置文件
│
├── 📂 Scripts/
│   ├── 📂 AOT/                    # 🔴 AOT (主包) 模块代码 (请勿修改)
│   │   └── 📂 Stubs/              #   ⚫️ 自动生成的存根代码，用于 AOT<=>Hotfix 调用
│   │                              #      (打包遇到疑难杂症时，可尝试清空此目录后重新生成)
│   └── 📂 Hotfix/                 # 🟢 所有热更新 C# 代码的大本营 (开发者主要工作区)
│       ├── 📂 GameLogic/          #   你的游戏业务逻辑 (示例)
│       ├── 📂 Core/               #   🔴 热更新框架核心代码 (请勿修改)
│       └── 📂 Managers/           #   🔴 热更新数据管理代码 (请勿修改)
│
├── 📂 Plugins/                    # 🟣 第三方插件目录
│
└── 📂 StreamingAssets/            # 🔴 首包资源存放目录，热更DLL和AB包会放入此处 (请勿修改)
```

## 🛠️ 构建流程说明

当您使用 `YEngine/Build`菜单时，框架在后台执行了以下关键步骤：

1.  **HybridCLR Pre-Build**:
    -   **生成补充元数据AOT Dll**: 分析热更代码，确定哪些 AOT 类型需要在运行时被访问。
    -   **生成桥接函数**: 创建 AOT 代码与热更代码互相调用的桥梁。
    -   **生成存根 (`Stubs`)**: 在 `Scripts/AOT/Stubs/` 目录下生成存根代码，让主包可以“假装”直接调用热更代码。

2.  **编译热更新代码**:
    -   将 `Assets/Scripts/Hotfix/` 目录下的所有 C# 代码编译成 `hotfix.dll` 文件。

3.  **构建 AssetBundles**:
    -   扫描 `Assets/GameRes_Hotfix/` 目录下的所有资源，并按照预设规则将它们打包成 AssetBundles。

4.  **资源与代码注入**:
    -   将编译好的 `hotfix.dll` 和打包好的 AssetBundles 复制到 `StreamingAssets` 目录，准备随安装包一起发布。

5.  **构建应用程序**:
    -   最后，调用 Unity 的 `BuildPlayer` 接口，生成最终的.exe 文件。

## 常见问题 (FAQ)

**Q: 我应该把我的游戏代码放在哪里？**  
A: 请统一放在 `Assets/Scripts/Hotfix/` 目录下，建议在里面创建自己的子文件夹，如 `GameLogic`、`Systems` 等。

**Q: 我应该把我的游戏资源（Prefab, Scene等）放在哪里？**  
A: 请统一放在 `Assets/GameRes_Hotfix/` 目录下，你可以自由创建子目录来组织它们。

**Q: 打包时遇到 AOT 相关的编译错误怎么办？**  
A: 绝大多数情况下是存根文件 (`Stubs`) 没有被正确更新。请尝试 **清空 `Assets/Scripts/AOT/Stubs/` 目录下的所有文件**，然后重新执行打包流程，框架会自动重新生成它们。

**Q: 为什么有些目录被标记为“请勿修改”？**  
A: 这些目录包含了框架的运行核心。修改它们可能会导致热更新流程中断或产生不可预知的错误。如果您需要扩展框架功能，建议通过继承和组合的方式，在您的业务逻辑代码中实现。

---
🟢捐赠作者🟢

![Image](https://github.com/user-attachments/assets/5f4ffab3-c976-4e08-86e6-f7997685ab41)![Image](https://github.com/user-attachments/assets/a56d6f2c-072a-4b9f-b18d-9775c485a7a5)
