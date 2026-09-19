# FluentGit

> A lightweight Git GUI client built on **WinUI 3 / Windows App SDK**  
> Pursuing a modern Fluent Design interface and smooth experience, focusing on daily repository management.

---

## ✨ Features

| Module | Feature |
|--------|---------|
| **Repository Management** | Initialize repository (`git init`), open local repository |
| **Remote Sync** | Pull updates (`git pull`), fetch remote info (`git fetch`) |
| **Commit Changes** | Stage all (`git add -A`), commit (`git commit -m`) |
| **Directory Browsing** | Directory tree view, double-click to open files, right-click menu (Open / Terminal / Explorer / Copy Path) |
| **Settings Center** | Auto-find Git path + signature verification, build artifact filter toggle, settings search |
| **Theme** | Follows system light/dark theme (Mica on Windows 11, solid color fallback on Windows 10) |
| **Notifications** | All operation results shown via InfoBar sliding notification, auto-dismiss |

> **Clone feature** is implemented in `GitService` layer (with progress and cancellation), but the UI entry is temporarily commented out, reserved for future restoration.

---

## 🖼️ Screenshots

> No screenshots yet, welcome to run and experience.

---

## 🧱 Tech Stack

- **UI**: WinUI 3 / Windows App SDK 2.4
- **Language**: C# / .NET 10
- **Packaging**: MSIX (self-signed)
- **Platform**: Windows 10 22H2 (19045) and above, Windows 11

---

## 📁 Project Structure

```
FluentGit/
├── Easteregg/
│   ├──You should go see for yourself.
├── Views/                          # Pages
│   ├── MainWindow.xaml             # Main window (Mica + custom TitleBar)
│   ├── RepoPage.xaml               # Repo management, sync, commit
│   ├── AdminPage.xaml              # Directory tree + context menu
│   ├── SettingsPage.xaml           # Settings center (search, grouping)
│   └── ...
├── Services/
│   ├── GitService/                 # Modular Git command wrapper
│   │   ├── GitService.cs           # Main entry + common methods
│   │   ├── GitService.Repository.cs
│   │   ├── GitService.Remote.cs
│   │   ├── GitService.Commit.cs
│   │   ├── GitService.Status.cs
│   │   ├── GitService.Branches.cs
│   │   └── GitService.Models.cs
│   ├── GitPathHelper.cs            # Auto-find Git path + signature verification
│   ├── SettingsService.cs          # JSON settings persistence
│   ├── AppLogger.cs                # Unified logging (OK / Warning / Error / Info)
│   ├── AppState.cs                 # Global state (current repo path)
│   └── Search/                     # Generic search module
│       ├── SearchEntry.cs
│       └── SearchService.cs
├── Assets/                         # Icon resources
└── ...
```

---

## 🚀 Build & Run

### Requirements

- Windows 10 22H2 (10.0.19045) or later
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Git for Windows](https://git-scm.com/install/windows) (official signed version)

### Command Line Build

```powershell
# Debug
dotnet build FluentGit.csproj -c Debug -p:Platform=x64

# Run
.\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\FluentGit.exe
```

Or use the included `Building.ps1` script:

```powershell
./Building.ps1
```

### Package as MSIX

```powershell
dotnet publish -c Release -p:Platform=x64 `
  -p:RuntimeIdentifier=win-x64 `
  -p:WindowsPackageType=MSIX `
  -p:WindowsAppSDKSelfContained=false `
  -p:GenerateAppxPackageOnBuild=true `
  -p:AppxPackageSigningEnabled=false
```

---

## 🔐 Security

- **Git path signature verification**: Uses WinVerifyTrust to validate the Authenticode signature of `git.exe`, `bash.exe`, `sh.exe`, etc. The signer must be **Johannes Schindelin** (official maintainer of Git for Windows).
- **Smart path resolution**: Supports deriving `git.exe` from `bash.exe` / `git-bash.exe` / `git-cmd.exe`.
- **Offline verification**: Uses `WTD_CACHE_ONLY_URL_RETRIEVAL`, no network dependency.

---

## 🧘 Developer Easter Egg

A **digital Buddha** is enshrined at the top of the `MainWindow.xaml.cs` constructor to bless the code with no BUGs.

> **Do NOT remove.** Tested: removing it increases BUG rate by 100%. (Just kidding... probably.)

---

## 🗺️ Roadmap

- [ ] Commit history (`git log`)
- [ ] Branch switching (`git checkout`)
- [ ] Push (`git push`)
- [ ] Built-in `.gitignore` editor
- [ ] Restore clone UI (progress bar + cancel)
- [ ] Multi-repository management

---

## 📄 License

MIT License

---

## 🙏 Credits

- [Git for Windows](https://gitforwindows.org/)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- All contributors and users

---
See [`Easteregg/README.en.md`](./Easteregg/README.en.md) for more Easter eggs.
---

> **FluentGit** — Making Git more elegant.