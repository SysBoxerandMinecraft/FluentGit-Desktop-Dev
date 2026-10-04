# 📜 发布仪式全流程 · Full Release Ritual

> “发版不是一件事。发版是一场仪式。”
> “A release is not a task. A release is a ritual.”

英文版：[Ritual.en.md](./Ritual.en.md)

---

## 一、缘起

FluentGit 还没有走到 v1.0.0。
这份《发布仪式全流程》的缘起，来自我以前另一个项目的 v0.9.0。

那个项目的 v0.9.0 发布那天，我没有做准备。

那是一个普通的周三下午。
CI 是绿的，测试是过的，代码看起来是干净的。
于是我按下了 dotnet publish -c Release。

七分钟之后，v0.9.0 上线。
十九分钟之后，用户开始报问题。
三十分钟之后，我发现数据库 schema 没同步。
四十五分钟之后，我发现 CHANGELOG.md 里写的是 v0.8.0。

那一天的教训，成了这份文档。

FluentGit 还没有发布过软件包。
但独自开发者没有发布经理。
我就是发布经理。
---

## 二、发布仪式与三炷香的关系

有人问：这份文档和 [`Incense.md`](./Incense.md) 有什么区别？

**三炷香，是仪式里的一个环节。**
**这份文档，是仪式本身。**

```text
Incense.md        →  发版前 5 分钟的那一炷香
Ritual.md         →  从 T-7 到 T+7 的完整流程
```

三炷香是**仪式感的浓缩**。
发布仪式是**仪式感的展开**。

两者都需要。
三炷香让你停下来。
发布仪式让你不漏掉。

---

## 三、发布仪式总览

FluentGit 的发布仪式，分为六个阶段。

| 阶段 | 时间 | 名称 | 核心任务 |
|:---:|:---:|---|---|
| 第一幕 | T-7 | **净** · Purification | 冻结功能，清理分支 |
| 第二幕 | T-3 | **备** · Preparation | 准备 Release Notes，整理版本号 |
| 第三幕 | T-1 | **诵** · Recitation | 通读 CHANGELOG，朗读版本宣言 |
| 第四幕 | T-0 | **行** · Action | 三炷香 + 发布 |
| 第五幕 | T+1 | **验** · Verification | 验证发布，监控异常 |
| 第六幕 | T+7 | **回** · Reflection | 回顾，复盘，回向 |

**六个阶段，每一个都不能跳。**

不是因为仪式需要完整。
是因为**每一个阶段，都对应一次真实的事故。**

---

## 四、第一幕 · 净 · T-7

### 4.1 净场

从 T-7 开始，**禁止合并新功能**。

```powershell
# 冻结 main 分支（通过 PR 描述公示）
echo "🔒 Feature Freeze from $(Get-Date -Format 'yyyy-MM-dd') to vX.Y.Z release" `
  >> RELEASE_FREEZE.md
```

**允许合并的：**
- Bug Fix（必须关联 issue）
- 测试补充
- 文档更新
- 本地化（如已就绪）

**不允许合并的：**
- 新功能
- 架构调整
- 依赖升级（除非安全修复）
- 重命名类 / 文件
- 修改公共接口

### 4.2 净分支

```powershell
# 查看所有本地分支
git branch -vv

# 查看所有远端分支
git fetch --all --prune
git branch -r

# 删除已合并的本地分支
git branch --merged main | ForEach-Object {
    if ($_ -notmatch '^\*|main|develop') {
        git branch -d $_.Trim()
    }
}
```

**净场的意义：**
发布前的每一个多余分支，都是一次误操作的机会。

### 4.3 净心

T-7 这天，**给自己 15 分钟**。

不是看代码，不是看 issue，不是看 Slack。
是看窗外的天。

**因为接下来的七天，你会很忙。**
**你需要记得，为什么当初要写这个东西。**

---

## 五、第二幕 · 备 · T-3

### 5.1 备 Release Notes

打开 `CHANGELOG.md`，按以下格式整理：

```markdown
## [v1.0.0] - 2025-03-15

### ✨ 新增
- 添加分支切换功能 (#112)
- 添加提交历史查看 (#118)

### 🐛 修复
- 修复 Mica 背景在 Win10 19045 上的闪白问题 (#87)
- 修复仓库路径包含中文时的解析错误 (#95)

### 🔧 改进
- 提升 RepoPage 加载速度约 40% (#101)
- 统一 AppLogger 日志格式 (#108)

### 📝 文档
- 补充 Building.ps1 使用说明
- 更新 Easteregg 索引

### ⚠️ 已知问题
- 跨时区仓库的时间显示仍有偏差，计划 v1.1 修复 (#123)
```

**规则：**
- **每一条改动，必须关联 issue 号。** 没有 issue 的改动，说明它不该被合并。
- **不写"修复了一些问题"。** 参见 [`Taboos.md`](./Taboos.md)。
- **不写"性能提升"。** 写具体数字。
- **已知问题必须公开。** 隐瞒已知问题，是发布欺诈。

### 5.2 备版本号

修改以下文件中的版本号：

```powershell
# 1. FluentGit.csproj
<Version>1.0.0</Version>

# 2. Package.appxmanifest
<Identity Version="1.0.0.0" />

# 3. CHANGELOG.md
## [v1.0.0] - 2025-03-15

# 4. README.md 中的版本徽章（如有）
```

**规则：**
- **版本号必须与 Tag 一致。**
- **版本号必须与 Release Notes 一致。**
- **版本号一旦写进 CHANGELOG，就不能再改。** 除非是笔误。

### 5.3 备回滚方案

**每一个发布，都必须有一个回滚方案。**

```markdown
## Rollback Plan for v1.0.0

### If the MSIX package is broken:
1. Unpublish from Microsoft Store (if published)
2. Restore previous MSIX from backup
3. Post-mortem issue opened

### If there is a critical bug:
1. Decide: hotfix or rollback?
2. If hotfix: branch from v0.9.0, apply minimal fix, re-release as v1.0.1
3. If rollback: restore previous version, tag the bug as v1.0.1 blocker

### Rollback Owner: @username
### Rollback Deadline: within 24 hours of discovery
```

**没有回滚方案的发布，不是发布。**
**是赌博。**

---

## 六、第三幕 · 诵 · T-1

### 6.1 通读 CHANGELOG

T-1 这天，**从头到尾读一遍 `CHANGELOG.md`**。

不是看，是**读**。
用嘴念出来。

**你会发现问题。**

- 某个功能描述写错了
- 某个 issue 号写错了
- 某个版本号写错了
- 某一条改动，你已经不记得是什么了

**最后一条最危险。**

如果你不记得那条改动是什么，那它可能是别人的改动。
**你必须搞清楚它。**

### 6.2 朗读版本宣言

**每一步发布，都有一句宣言。**

v1.0.0 的宣言是：

> “FluentGit，从今天起，可以被一个从未用过它的人，从零开始使用。”
> “FluentGit, from today, can be used from scratch by someone who has never used it.”

这句话不是营销。
这句话是**发布的门槛**。

**如果你的版本号是 1.0.0，那么它必须能说这句话。**
**如果你说不出这句话，那你的版本号应该还是 0.x。**

### 6.3 复核三炷香

T-1 的晚上，**试一次三炷香**。

不是真的发版，是**走一遍流程**：

```powershell
# 1. 关闭所有 IDE
# 2. 执行
dotnet clean
dotnet build -c Release -p:Platform=x64
dotnet test  -c Release --no-build
# 3. 检查 git status
git status
# 4. 假打一个 tag（本地，不 push）
git tag -a v1.0.0-test -m "Pre-release dry run"
# 5. 删除这个 tag
git tag -d v1.0.0-test
```

**这一遍叫做“预演”。**
**没有预演的发布，是撞大运。**

---

## 七、第四幕 · 行 · T-0

### 7.1 正式准备

T-0 这天早上，**在开始之前**，做四件事：

```text
[ ] 关闭所有 IDE
[ ] 关闭 Slack / Teams / 钉钉
[ ] 关闭浏览器里所有 StackOverflow 标签页
[ ] 泡一杯咖啡（可选）
```

### 7.2 三炷香

完整流程见 [`Incense.md`](./Incense.md)。

简要版：

```text
第一炷 · 敬代码    →  dotnet build -c Release → 0 error 0 warning
第二炷 · 敬测试    →  dotnet test -c Release  → all pass
第三炷 · 敬未来    →  git status → clean, CHANGELOG 已更新，版本号已更新
```

三炷香烧尽之后，再执行下一步。

### 7.3 打 Tag

```powershell
# 确认当前分支正确
git branch --show-current  # 应输出 main 或 release/vX.Y.Z

# 确认最新 commit 是你要发布的
git log --oneline -5

# 打一个带注释的 tag（必须带注释）
git tag -a v1.0.0 -m "Release v1.0.0 - Full Release Ritual"

# 检查 tag 内容
git show v1.0.0
```

**Tag 打错，是发布仪式里最不可原谅的错误。**
参见 [`Taboos.md`](./Taboos.md)。

### 7.4 发布

```powershell
dotnet publish -c Release -p:Platform=x64 `
  -p:RuntimeIdentifier=win-x64 `
  -p:WindowsPackageType=MSIX `
  -p:WindowsAppSDKSelfContained=false `
  -p:GenerateAppxPackageOnBuild=true `
  -p:AppxPackageSigningEnabled=false
```

**在命令执行的过程中：**

- **不要切窗口。**
- **不要回消息。**
- **不要在心里预演接下来要说什么。**
- **就看着输出。**

**看输出，是一种修行。**

### 7.5 推送

```powershell
# 推送代码
git push origin main

# 推送 tag
git push origin v1.0.0
```

**推送 tag 是最后一步。**
**推完 tag，就不要再改任何东西。**

### 7.6 公布

```markdown
# Release v1.0.0

We are pleased to announce FluentGit v1.0.0.

Release notes: [CHANGELOG.md](link)
Download: [GitHub Releases](link)

Special thanks to all contributors.

> "FluentGit, from today, can be used from scratch by someone who has never used it."
```

**公告里不写：**
- “这是最后一个改动”
- “应该没问题”
- “在我机器上是好的”

**公告里必须写：**
- 版本号
- 发布日期
- 已知问题（如果有）
- 下载链接

---

## 八、第五幕 · 验 · T+1

### 8.1 验证发布

发布后 24 小时内，做以下验证：

```text
[ ] 从官方渠道下载 MSIX 包
[ ] 在一台干净的 Windows 10 22H2 上安装
[ ] 在一台干净的 Windows 11 上安装
[ ] 打开应用，确认窗口正常显示
[ ] 初始化一个测试仓库
[ ] 执行一次 pull / commit / push
[ ] 确认 InfoBar 提示正常
[ ] 检查 Mica 背景（Win11）
[ ] 检查回退背景（Win10）
```

**任何一项失败，立即启动回滚方案。**

### 8.2 监控异常

```text
[ ] 查看 Issue 列表，是否有新报的 Bug
[ ] 查看 CI 状态，是否有异常失败
[ ] 查看日志系统，是否有新增的 Error 级别日志
[ ] 查看用户群 / 论坛，是否有负面反馈
```

**发布后的 24 小时，是最脆弱的 24 小时。**
**不要在这 24 小时内做任何其他重大改动。**

### 8.3 回滚决策

如果 T+1 出现严重问题，有两个选择：

**选择 A · Hotfix**
- 从当前版本 branch
- 修一个最小改动
- 发 v1.0.1

**选择 B · Rollback**
- 撤回当前版本
- 恢复到上一个稳定版本
- 复盘，重新准备

**决策标准：**

| 问题等级 | 处理方式 |
|---|---|
| 无法启动 | 立即 Rollback |
| 数据损坏 | 立即 Rollback |
| 严重功能不可用 | Hotfix（24 小时内） |
| 一般 Bug | 记录，v1.0.1 修复 |
| UI 微调 | 记录，下个版本 |

**回滚不是失败。**
**回滚是对用户的负责。**

---

## 九、第六幕 · 回 · T+7

### 9.1 复盘

T+7 这天，**团队坐在一起**，问四个问题：

1. **这次发布，哪里做得好？**
2. **这次发布，哪里可以更好？**
3. **有没有违反任何一条禁忌？**（见 [`Taboos.md`](./Taboos.md)）
4. **下次发布，改什么？**

**复盘的结论，写成文档，作为下一次发布的输入。**

### 9.2 归档

```powershell
# 归档本次发布的相关资料
mkdir docs/releases/v1.0.0
cp CHANGELOG.md docs/releases/v1.0.0/
cp RELEASE_NOTES.md docs/releases/v1.0.0/
cp ROLLBACK_PLAN.md docs/releases/v1.0.0/
# 提交
git add docs/releases/v1.0.0
git commit -m "docs: archive v1.0.0 release materials"
```

**归档不是为了留痕。**
**归档是为了让三个月后的自己，不用重新猜。**

### 9.3 回向

T+7 的晚上，**放松一下**。

不要看代码。
不要看 issue。
不要看 Slack。

**你刚刚完成了一次发布。**
**这值得一顿好饭。**

---

## 十、禁忌条款

### 第一诫 · 不可跳过 T-7

不得在 T-0 才开始冻结功能。

**功能冻结必须提前七天。**
**七天，是让所有未完成的东西，被安静地搁置的时间。**

### 第二诫 · 不可在 T-3 之后加功能

T-3 之后，只允许修 Bug。

**任何“顺便加个小功能”的想法，都是 v1.0.1 的内容。**

### 第三诫 · 不可在 T-1 修改 CHANGELOG 之外的东西

T-1 只做三件事：
1. 读 CHANGELOG
2. 读版本宣言
3. 预演三炷香

**不要在这一天改代码。**

### 第四诫 · 不可跳过预演

T-1 晚上的预演，不能跳过。

**预演的价值，就在于它会提前暴露你没准备的东西。**

### 第五诫 · 不可在 T-0 改代码

T-0 那天，**不写新代码**。

如果发现 Bug，选择：
- **Bug 小**：记录，v1.0.1 修复
- **Bug 大**：取消发布，回到 T-1

**T-0 改代码，等于毁掉整场仪式。**

### 第六诫 · 不可在 T+1 做新改动

T+1 是**验证日**，不是**开发日**。

**除非有 Hotfix 需求，否则不要在 T+1 改动任何东西。**

### 第七诫 · 不可跳过 T+7 复盘

不得因为“这次很顺利”就跳过复盘。

**顺利的发布，才是最值得复盘的发布。**
**因为它告诉你：哪些做对了。**

---

## 十一、功德与回向

| 行为 | 功德 |
|---|---:|
| 完整执行六个阶段 | +66 |
| T-7 准时冻结功能 | +10 |
| T-3 备好 Release Notes 和回滚方案 | +15 |
| T-1 预演一次三炷香 | +10 |
| T-0 一次发布成功 | +50 |
| T+1 24 小时内完成验证 | +20 |
| T+7 认真复盘 | +30 |
| 跳过 T-7 | −30 |
| T-0 改代码 | −100 |
| T+1 出现严重问题但及时回滚 | +5（态度尚可） |
| 无回滚方案就发布 | −200 |
| 跳过 T+7 复盘 | −50 |

### 回向

> 愿此仪式，
> 净七日之心，
> 备三日之细，
> 诵一日之言，
> 行一刻之定，
> 验一日之稳，
> 回七日之明。
>
> 愿此发布，
> 一次成功，无人失眠。
> 若未能，
> 愿回滚顺利，复盘有得。

---

## 十二、常见问题

**Q1：六个阶段，会不会太重？**
A：不会。
**六个阶段，是六个接口。**
**接口不是负担，是边界。**

一周的仪式，换来的是一次安静的发布。
**值得。**

**Q2：如果项目很小，也要走完整流程吗？**
A：不。
**小项目可以压缩时间，但不能压缩阶段。**

```text
大项目：T-7  →  T-3  →  T-1  →  T-0  →  T+1  →  T+7
小项目：T-2h →  T-1h →  T-30m →  T-0  →  T+30m →  T+1d
```

**时间可以压缩，阶段不能跳过。**

**Q3：如果老板要求跳过 T-7？**
A：可以，但请老板**书面**确认。
**书面确认的东西，才是责任。**

**Q4：如果 T-1 预演失败，怎么办？**
A：推迟发布。
**预演失败，意味着有一些东西你没准备好。**
**推迟一天，比发布后出事故，便宜得多。**

**Q5：如果 T+1 出现严重问题，我可以立刻改代码吗？**
A：可以，但要遵守 Hotfix 流程：
1. 从当前 tag branch
2. 只做最小改动
3. 发布 v1.0.1

**不要在主分支上直接改。**
**主分支的每一次直接提交，都是下一次发布事故的种子。**

**Q6：T+7 复盘，谁参加？**
A：**参与过本次发布的所有人。**
包括：
- 写代码的
- 测代码的
- 打包的
- 写文档的
- 决策的

**没有旁观者。所有人都是当事人。**

**Q7：复盘时，可以批评人吗？**
A：不可以批评人。
**可以批评流程、批评决策、批评工具。**
**不可以批评某个人。**

**复盘的目的，是改进系统，不是找人负责。**

**Q8：为什么最后一个阶段叫“回”？**
A：因为发布不是终点。
**发布是一个循环的起点。**

T+7 之后，你会回到 T-7 的下一次。
**发布，是永远的轮回。**

---

## 结语

发布仪式不是教条。

它是**一群人，在一次次事故之后，共同总结出的一张地图**。

这张地图，不一定完美。
但它至少告诉你：**哪里是悬崖。**

**你可以选择不走这张地图。**
**但你走的时候，请记得回头看一眼。**

看看那些曾经在悬崖边掉下去的人，
**他们是用什么代价，换来了这张地图。**

---

> 南无本师释迦牟尼佛
> 南无本师释迦牟尼佛
> 南无本师释迦牟尼佛
>
> 一净，一备，一诵，
> 一行，一验，一回。
>
> 愿此发布，
> 六幕圆满，
> 无事生非，
> 无人无眠。

---

*仪式位置：从 T-7 到 T+7 的每一天*
*执行频率：每次正式发布*
*守护状态：常驻*
*功德：见第十一章*
