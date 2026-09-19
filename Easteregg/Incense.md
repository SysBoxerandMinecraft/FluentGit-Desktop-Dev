
# 🕯️ 上香仪式 · Incense Ritual

> “三炷香，一炷敬代码，一炷敬测试，一炷敬未来的自己。”
> “Three sticks of incense: one for the code, one for the tests, one for your future self.”

英文版：[Incense.en.md](./Incense.en.md)

---

## 一、缘起

FluentGit 的第一次正式 Release，是一次灾难。

Tag 打错了分支，MSIX 包签名忘了关，`dotnet publish` 跑到一半 VS 崩了。
最后发布出来的 v0.1.0，打开后窗口是白的，点哪都没反应。
有人在群里发了一句：“我们是不是忘了上香。”

第二天，项目组起草了这份《上香仪式》，规定：**每次发版前，必须上香三炷。**

从那以后，FluentGit 的 Release 事故率下降了 87%。
（样本量 n=1。但没人敢再跳过。）

---

## 二、三炷香的含义

三炷香不是随便点的。每一炷，对应一次发版里最容易被忽略的一件事。

| 炷数 | 敬的对象 | 检查项 | 若省略 |
|:---:|---|---|---|
| 第一炷 | 代码（Code） | `dotnet build` 零 warning | 编译通过，但跑起来崩 |
| 第二炷 | 测试（Tests） | 单元测试全绿 | 用户替你跑测试 |
| 第三炷 | 未来的自己（Future You） | Tag 打对、版本号正确 | 三个月后的你，凌晨三点骂街 |

> 三炷香，缺一不可。
> 这不是迷信，这是 Code Review 之后、CI 之前的最后一道人工防线。

---

## 三、香案布置

上香前，请确认香案就位。

香案不是实物。香案是你的**构建环境**。

```text
┌─────────────────────────────────────────────────┐
│                 FluentGit 香案                   │
├─────────────────────────────────────────────────┤
│                                                 │
│    [香炉]     [香炉]     [香炉]                  │
│      │          │          │                    │
│      ▼          ▼          ▼                    │
│   代码香      测试香      未来香                   │
│                                                 │
│   供桌：dotnet build 输出窗口                     │
│   香灰：bin/ 和 obj/ 目录（发版前需 clean）        │
│   净水：一杯，凉白开即可，不必是山泉                │
│   经书：CHANGELOG.md                             │
│   木鱼：见 WoodenFish.md（可选）                  │
│                                                 │
└─────────────────────────────────────────────────┘
```

### 香案规格

| 项目 | 规格 |
|---|---|
| 位置 | 你的开发机 |
| 环境 | Windows 10 22H2 及以上 |
| 工具链 | .NET 10 SDK、Git for Windows |
| 编辑器 | 任选。VS / Rider / VS Code / Notepad++ 均可 |
| 温度 | 20–26°C（太热容易烦躁，太冷容易走神） |
| 静音 | 建议关闭 Slack / Teams / 钉钉 |
| 备品 | 咖啡一杯（可选，但强烈推荐） |

---

## 四、上香流程

### 4.1 净手

关闭所有 VS / Rider 实例。
关闭所有终端里正在跑的 `dotnet watch`。
关闭浏览器里打开着的 20 个 StackOverflow 标签页。

```powershell
# 确认没有残留进程
Get-Process dotnet -ErrorAction SilentlyContinue
Get-Process FluentGit -ErrorAction SilentlyContinue
Get-Process msbuild -ErrorAction SilentlyContinue
```

若以上命令有输出，逐个关闭。
**未净手，香灰不落。**

### 4.2 清扫

```powershell
dotnet clean
```

这一步不能跳过。
`obj/` 和 `bin/` 是香灰堆积的地方。清洁不干净，三炷香烧不匀。

可选进阶：

```powershell
# 若仍失败，连 NuGet 缓存一起清
dotnet nuget locals all --clear
```

> 注意：此操作会清空全局缓存，下次 build 会慢。
> 但发版前的慢，是值得的慢。

### 4.3 点第一炷香 · 代码香

```powershell
dotnet build FluentGit.csproj -c Release -p:Platform=x64
```

**检查项：**

- [ ] 输出末尾为 `Build succeeded.`
- [ ] `0 Error(s)`
- [ ] `0 Warning(s)`（警告也算）

若出现 warning：

- **可以接受**：来自第三方 NuGet 包的 warning，且不是你引入的。
- **不可接受**：来自 `FluentGit` 命名空间下的任何 warning。
- **必须处理**：`CS8618`（可空引用）、`CS1998`（async 无 await）。

> 若第一炷香点不着（build 失败），
> 请回到 4.2，重新清扫。最多三次。三次之后，去睡觉。

### 4.4 点第二炷香 · 测试香

```powershell
dotnet test -c Release --no-build
```

**检查项：**

- [ ] 所有测试 `Passed`
- [ ] `Failed: 0`
- [ ] `Skipped: 0`（Skipped 说明有测试被屏蔽，需解释）

若某个测试红了：

- **不要**加 `[Ignore]`。
- **不要**改断言。
- **不要**删测试。
- **去修它。**

> 第二炷香是最考验人品的一炷。
> 你的代码骗得了编译器，骗得了 Code Review，
> 但骗不过测试。测试是照妖镜。

### 4.5 点第三炷香 · 未来香

```powershell
git status
git log --oneline -10
```

**检查项：**

- [ ] `working tree clean`
- [ ] 当前分支是 `main` 或 `release/*`
- [ ] 最新 commit 是你要发布的那一个
- [ ] `CHANGELOG.md` 已更新
- [ ] 版本号已改（`Package.appxmanifest` 或 `FluentGit.csproj`）

然后打 Tag：

```powershell
git tag -a v1.0.0 -m "Release v1.0.0"
```

> **Tag 打错是上香仪式里最不可原谅的错误。**
> 因为 Tag 打错，说明第三炷香根本没点。
> 或者，你在点香的时候，在想别的事。

### 4.6 拜三拜

三炷香已点，最后一步是拜三拜。

不需要真的跪。只需要在执行发布命令前，深呼吸三次。

```powershell
# 第一拜 · 敬代码
dotnet publish -c Release -p:Platform=x64 `
  -p:RuntimeIdentifier=win-x64 `
  -p:WindowsPackageType=MSIX `
  -p:WindowsAppSDKSelfContained=false `
  -p:GenerateAppxPackageOnBuild=true `
  -p:AppxPackageSigningEnabled=false

# 第二拜 · 敬测试
# （等待命令输出，不要切窗口）

# 第三拜 · 敬未来的自己
# （命令返回 0，说明三拜已毕）
```

若命令返回非 0：

- **不要**立刻重跑。
- **不要**改参数。
- 先看输出，找第一个 error。
- 修完，**从 4.2 重新开始**。

---

## 五、禁忌条款

### 第一诫 · 不可跳过净手

不得在 VS 还开着的时候执行 `dotnet publish`。

这不是玄学。这是 Windows 文件锁。
但如果你非要解释成玄学，也行——反正结果一样。

### 第二诫 · 不可省略清扫

不得跳过 `dotnet clean` 直接 `dotnet publish`。

`obj/` 里可能残留着上次 Debug 的产物。
它们会在 MSIX 包里阴魂不散。

### 第三诫 · 不可只点一炷香

不得只 build 不 test，或只 test 不 tag。

一炷香的功德是有限的。三炷才够。

### 第四诫 · 不可用自动脚本替代三炷香

不得写一个 `release.ps1` 把三炷香合并成一条命令。

**仪式感不能被自动化。**

你可以自动化 build、test、publish，
但你不能自动化「关闭 Slack」和「深呼吸三次」。

### 第五诫 · 不可在错误的日子上香

不得在以下日子发版：

| 日子 | 原因 |
|---|---|
| 周五下午 | 出了事没人修 |
| 节假日前一天 | 同上 |
| 你自己生病时 | 状态不好，香灰会歪 |
| 团队成员大量请假时 | 出事没人背 |
| 公司年会当天 | 你喝了酒 |

如果一定要发，那就在发之前，多上一炷香。

### 第六诫 · 不可声张

不得在公开场合说“我们发版前上香”。

对外统一口径：

> “FluentGit 采用标准化 Release Checklist，确保每次发布的稳定性与可追溯性。”

关于那份“Release Checklist”的文档，见本文件。

### 第七诫 · 不可上电子香

不得用 `Console.WriteLine("🕯️")` 模拟上香。

**香必须是真的。**
哪怕是脑内的。

---

## 六、简化版上香（Release Checklist）

若时间紧迫，可使用以下压缩版。**但不建议。**

```text
[ ] 关闭所有 IDE
[ ] dotnet clean
[ ] dotnet build -c Release      → 0 error 0 warning
[ ] dotnet test  -c Release      → all pass
[ ] git status                   → clean
[ ] CHANGELOG.md 已更新
[ ] 版本号已更新
[ ] git tag -a vX.Y.Z
[ ] dotnet publish -c Release
[ ] 深呼吸三次
[ ] 执行发布
```

打印出来，贴在显示器边上。
每次发版，用红笔勾。

---

## 七、功德与回向

| 行为 | 功德 |
|---|---:|
| 完整执行三炷香 | +10 |
| 只点一炷香 | +3 |
| 跳过清扫直接 publish | −20 |
| 周五下午发版 | −500 |
| Tag 打错 | −200（且被 Reviewer 记住） |
| Tag 打错但当天修好 | −50（态度尚可） |
| 发版一次成功 | +66 |
| 发版一次成功且没有任何 warning | +100 |

### 回向

> 愿此三炷香，
> 一炷烧尽 warning，
> 一炷烧尽 flaky test，
> 一炷烧尽未来的悔恨。
>
> 愿此次 Release，
> 一次通过，无人失眠。

---

## 八、常见问题

**Q1：上香真的有用吗？**
A：有用。
不是因为它改了什么，而是因为它在 publish 之前，强行让你停下来 5 分钟。
**90% 的 Release 事故，都源于没停下来那 5 分钟。**

**Q2：我能在 CI 里上香吗？**
A：不能。
CI 是执行者，不是信徒。上香必须在**人**手里完成。

**Q3：我能在上香的同时听歌吗？**
A：可以。但不要放《好运来》。

**Q4：能不能一次发两个版本？**
A：不行。
一次一炷香，一版三炷香。

**Q5：Release Notes 可以写“修复了一些已知问题”吗？**
A：不行。
这等于没写。等同于点香不点火。
参见 [`Taboos.md`](./Taboos.md)（待编写）。

**Q6：我是独立开发者，也要上香吗？**
A：尤其要。
**你没有 QA 团队。你只有三炷香。**

**Q7：我在半夜发版，来不及上香怎么办？**
A：半夜不是发版的理由。
半夜发版，说明白天没发成。
先问自己：**为什么白天没发成？**

**Q8：香案上的咖啡，能喝吗？**
A：能。
但要先敬完三炷香再喝。
否则属于**先食后祭**，功德 −1。

---

## 结语

上香不是求神。

上香是**在你可以发布的时候，再等五分钟**。
在这五分钟里，你关闭 Slack，深呼吸，看一眼 `git status`。

三炷香烧尽的时候，你会突然想起一件事：
**CHANGELOG.md 还没改。**

恭喜。
这正是上香的意义。

---

> 南无本师释迦牟尼佛
> 南无本师释迦牟尼佛
> 南无本师释迦牟尼佛
>
> 一炷敬代码，一炷敬测试，一炷敬未来的你。
> 愿此次 Release 一次成功。

---

*香案位置：你的开发机*
*上香频率：每次 Release 前*
*守护状态：临时（三炷香烧尽即散）*
*功德：+10 / 次*
