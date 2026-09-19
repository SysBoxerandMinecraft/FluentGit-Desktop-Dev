# FluentGit

> 一个基于 **WinUI 3 / Windows App SDK** 的轻量级 Git 图形客户端  
> 追求 Fluent Design 的现代界面与流畅体验，专注日常仓库管理。

---

[English](README.EN.md) | 中文

## ✨ 功能特性

| 模块 | 功能 |
|------|------|
| **仓库管理** | 初始化仓库（`git init`）、打开本地仓库 |
| **远程同步** | 拉取更新（`git pull`）、获取远程信息（`git fetch`） |
| **提交变更** | 全部暂存（`git add -A`）、提交（`git commit -m`） |
| **目录浏览** | 目录树展示、双击打开文件、右键菜单（打开 / 终端 / 资源管理器 / 复制路径） |
| **设置中心** | Git 路径自动查找 + 签名验证、编译产物过滤开关、设置项搜索 |
| **主题** | 跟随系统亮 / 暗主题（Windows 11 启用 Mica，Windows 10 回退纯色） |
| **提示反馈** | 所有操作结果通过 InfoBar 滑入提示，自动消失 |

> **克隆功能** 已实现于 `GitService` 层（支持进度与取消），但 UI 入口暂时注释，保留后续恢复。

---

## 🖼️ 界面预览

> 暂未提供截图，欢迎运行体验。

---

## 🧱 技术栈

- **UI**：WinUI 3 / Windows App SDK 2.4
- **语言**：C# / .NET 10
- **打包**：MSIX（自签名）
- **平台**：Windows 10 22H2（19045）及以上、Windows 11

---

## 📁 项目结构

```
FluentGit/
├── Easteregg/
│   ├──你应该自己去看看。
├── Views/                          # 页面层
│   ├── MainWindow.xaml             # 主窗口（Mica + 自定义 TitleBar）
│   ├── RepoPage.xaml               # 仓库管理、同步、提交
│   ├── AdminPage.xaml              # 目录树 + 右键菜单
│   ├── SettingsPage.xaml           # 设置中心（搜索、分组）
│   └── ...
├── Services/
│   ├── GitService/                 # Git 命令模块化封装
│   │   ├── GitService.cs           # 主入口 + 通用方法
│   │   ├── GitService.Repository.cs
│   │   ├── GitService.Remote.cs
│   │   ├── GitService.Commit.cs
│   │   ├── GitService.Status.cs
│   │   ├── GitService.Branches.cs
│   │   └── GitService.Models.cs
│   ├── GitPathHelper.cs            # Git 路径自动查找 + 签名验证
│   ├── SettingsService.cs          # JSON 配置持久化
│   ├── AppLogger.cs                # 统一日志（OK / Warning / Error / Info）
│   ├── AppState.cs                 # 全局状态（当前仓库路径）
│   └── Search/                     # 通用搜索模块
│       ├── SearchEntry.cs
│       └── SearchService.cs
├── Assets/                         # 图标资源
└── ...
```

---

## 🚀 构建与运行

### 环境要求

- Windows 10 22H2 (10.0.19045) 或更高
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Git for Windows](https://git-scm.com/install/windows)（官方签名版）

### 命令行构建

```powershell
# Debug
dotnet build FluentGit.csproj -c Debug -p:Platform=x64

# 运行
.\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\FluentGit.exe
```

或使用附带的 `Building.ps1` 脚本：

```powershell
./Building.ps1
```

### 打包 MSIX

```powershell
dotnet publish -c Release -p:Platform=x64 `
  -p:RuntimeIdentifier=win-x64 `
  -p:WindowsPackageType=MSIX `
  -p:WindowsAppSDKSelfContained=false `
  -p:GenerateAppxPackageOnBuild=true `
  -p:AppxPackageSigningEnabled=false
```

---

## 🔐 安全说明

- **Git 路径签名验证**：通过 WinVerifyTrust 验证 `git.exe`、`bash.exe`、`sh.exe` 等文件的 Authenticode 签名，签名者需为 **Johannes Schindelin**（Git for Windows 官方维护者）。
- **智能路径推导**：支持从 `bash.exe` / `git-bash.exe` / `git-cmd.exe` 推导出 `git.exe`。
- **不联网校验**：签名验证使用 `WTD_CACHE_ONLY_URL_RETRIEVAL`，不依赖网络。

---

## 🧘 开发者彩蛋

`MainWindow.xaml.cs` 的构造函数顶部供奉了一尊 **电子佛祖**，用于保佑代码无 BUG。

> **请勿移除**。实测移除后 BUG 率上升 100%。（开玩笑的……大概）

---

## 🗺️ 开发计划

- [ ] 提交历史（`git log`）
- [ ] 分支切换（`git checkout`）
- [ ] 推送（`git push`）
- [ ] 内置 `.gitignore` 编辑器
- [ ] 恢复克隆 UI（进度条 + 取消）
- [ ] 多仓库管理

---

## 📄 开源协议

MIT License

---

## 🙏 鸣谢

- [Git for Windows](https://gitforwindows.org/)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- 所有贡献者与用户
---
更多彩蛋见 [`Easteregg/README.md`](./Easteregg/README.md)。
---

> **FluentGit** — 让 Git 更优雅。