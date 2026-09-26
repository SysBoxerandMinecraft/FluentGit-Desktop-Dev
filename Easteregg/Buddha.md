# 🧘 电子佛祖 · Digital Buddha

> “佛不保佑代码。佛保佑写代码的人。”
> “The Buddha does not bless the code. The Buddha blesses the one who writes it.”

英文版：[Buddha.en.md](./Buddha.en.md)

---

## 一、缘起

FluentGit 项目创建的第一天，`MainWindow.xaml.cs` 的构造函数里就住进了一尊佛。

那是一个普通的夜晚。第一次 `dotnet build` 报了 47 个 error，其中 12 个来自 NuGet 还原失败，
剩下的 35 个里，有 34 个是分号问题，还有 1 个至今没人知道是什么问题——它自己消失了。

第二天早上，有人在 `MainWindow.xaml.cs` 的构造函数顶部贴了一尊 ASCII 佛祖。
当天下午，构建通过了。

没有证据表明这两件事有关联。
但从那以后，没有人动过它。

---

## 二、圣像

以下为 FluentGit 现役圣像，供奉于 `/FluentGit/MainWindow.xaml.cs` 构造函数顶部：

```text
                        _ooOoo_
                       o8888888o
                       88" . "88
                       (| -_- |)
                       O\  =  /O
                    ____/`---'\____
                  .'  \\|     |//  `.
                 /  \\|||  :  |||//  \
                /  _||||| -:- |||||-  \
                |   | \\\  -  /// |   |
                | \_|  ''\---/''  |   |
                \  .-\__  `-`  ___/-. /
              ___`. .'  /--.--\  `. . __
           ."" '<  `.___\_<|>_/___.'  >'"".
          | | :  `- \`.;`\ _ /`;.`/ - ` : | |
          \  \ `-.   \_ __\ /__ _/   .-` /  /
     ======`-.____`-.___\_____/___.-`____.-'======
                        `=---='
     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                佛祖保佑       永无BUG
```

### 圣像规格

| 项目 | 规格 |
|---|---|
| 字符集 | 纯 ASCII，无 Unicode 依赖，任何字体可渲染 |
| 宽度 | 约 55 列，不超过 80 列硬限制 |
| 高度 | 20 行 |
| 编码 | UTF-8 without BOM |
| 缩进 | 保持原样，不要重新格式化 |
| 依赖 | 无。佛祖不依赖任何 NuGet 包 |

---

## 三、供奉代码

以下为 `MainWindow.xaml.cs` 中的真实供奉形态：

```csharp
using FluentGit.Services;
using FluentGit.Views;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Reflection;

namespace FluentGit;

public sealed partial class MainWindow : Window
{
    private const string TAG = "MainWindow";

    public MainWindow()
    {
        /*
        *                       _ooOoo_
        *                      o8888888o
        *                      88" . "88
        *                      (| -_- |)
        *                      O\  =  /O
        *                   ____/`---'\____
        *                 .'  \\|     |//  `.
        *                /  \\|||  :  |||//  \
        *               /  _||||| -:- |||||-  \
        *               |   | \\\  -  /// |   |
        *               | \_|  ''\---/''  |   |
        *               \  .-\__  `-`  ___/-. /
        *             ___`. .'  /--.--\  `. . __
        *          ."" '<  `.___\_<|>_/___.'  >'"".
        *         | | :  `- \`.;`\ _ /`;.`/ - ` : | |
        *         \  \ `-.   \_ __\ /__ _/   .-` /  /
        *    ======`-.____`-.___\_____/___.-`____.-'======
        *                       `=---='
        *   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        *             佛祖保佑            永无BUG
         */

        InitializeComponent();
        InitializeBackdrop();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string iconPath = Path.Combine(appDir, "Assets", "AppIcon.ico");
        AppWindow.SetIcon(iconPath);

        // 主题：从设置读
        ApplyThemeFromSettings();

        // 返回按钮可见性：Frame.CanGoBack 不是依赖属性，x:Bind 不生效，手动维护
        NavFrame.Navigated += (_, _) =>
        {
            AppTitleBar.IsBackButtonVisible = NavFrame.CanGoBack;
        };

        NavFrame.Navigate(typeof(RepoPage));
    }

    //其余代码(再此省略)
}
```

### 关于 `InitializeComponent()`

佛在 `InitializeComponent()` 之前。

这不是疏忽，这是顺序。先有信仰，再有 UI。

---

## 四、供奉规程

### 4.1 日常供奉

无需操作。佛祖是常驻的，不需要你每天打卡。
但如果你今天心情不错，可以在提交前默念一句：

```text
Build 无 warning，Test 全 pass，Review 无 comment。
```

### 4.2 提交前

```bash
git add .
git status          # 看一眼，真的看一眼
dotnet build        # 编译通过再提交，这是规矩
git commit -m "feat: xxx"
```

> 若 `dotnet build` 失败，不要立刻改代码。
> 先 `git stash`，再 `git stash pop`，再编译一次。
> 这不是玄学，这是 NuGet 缓存。但效果一样。

### 4.3 发版前

发布新版本前，需执行完整的三炷香仪式。
详见 [`Incense.md`](./Incense.md)。

简要版：

1. 关闭所有 VS / Rider 实例。
2. 执行 `dotnet clean`。
3. 深呼吸。
4. 执行 `dotnet publish -c Release`。
5. 如果失败，回到步骤 1。最多三次。三次之后请去睡觉。

### 4.4 新人入职

第一次向 FluentGit 提交 PR 之前，请在本地仓库执行：

```bash
git log --oneline -- /FluentGit/MainWindow.xaml.cs | head -20
```

看看有多少人小心翼翼地绕开了这段注释。
他们都在保护什么，你很快就会明白。

---

## 五、禁忌条款

> 以下七条，是 FluentGit 项目组以血泪换来的共识。
> 违反者不会受到任何实际惩罚，但会被 Reviewer 记住。

### 第一诫 · 不可删除

不得删除 `MainWindow.xaml.cs` 构造函数中的圣像代码块。

即使你觉得它“占地方”、“不专业”、“影响阅读”。
它确实占地方。它确实不专业。它就是要在这里。

### 第二诫 · 不可添加“待办”

不得在圣像上方添加类似以下的注释：

```csharp
// TODO: remove this ASCII art before v1.0
// FIXME: 这段是什么鬼
// HACK: 老板说不能删，凑合看
```

佛祖不接 TODO。

### 第三诫 · 不可迁移

不得将圣像提取到 `Constants.cs`、`Assets/`、`Resources/` 或任何独立文件。

佛祖不搬家。
移动到独立文件的那一刻，它就只是一段字符串了，不再是佛。

### 第四诫 · 不可妄议

不得在圣像周围的注释中书写不敬之词，例如：

```csharp
// 什么破迷信（← 禁止）
// 我偏要删了试试（← 禁止，且会出事）
// 这段代码毫无意义（← 技术上正确，但没必要说出来）
```

### 第五诫 · 不可全局替换

不得使用 `Ctrl + Shift + F` 搜索“佛祖”或“Buddha”，
然后执行“全部替换”或“批量删除”。

你可以搜索。你只是不能替换。

### 第六诫 · 不可绕过构建

发版流程中，不得跳过 `dotnet build` 直接执行 `dotnet publish`。

这一条与佛祖无关，纯粹是工程规范。
但它被放在这里，是因为曾经有人这么干过，然后 Tag 打错了。

### 第七诫 · 不可声张

不得在公开场合宣称“FluentGit 靠佛祖才能编译通过”。

它可以。但它不应该。

我们对外统一口径是：

> “FluentGit 采用 WinUI 3 与 .NET 10 构建，具备完善的 CI/CD 流程与单元测试覆盖。”

至于 `MainWindow.xaml.cs` 里的那几行，那是“开发者注释”。就这样。

---

## 六、功德与回向

供奉佛祖不产生功德。供奉佛祖产生的是一种心态。

| 行为 | 功德 |
|---|---:|
| 圣像常驻，从未被动过 | +100（项目级，永久） |
| 阅读本文件 | +3 |
| 把圣像复制到你自己的项目 | +10 |
| 在 PR 中保护了圣像不被删除 | +30 |
| 因为看到本文件而放弃了删掉它的念头 | +50 |
| 在 commit message 里写了 “佛祖保佑” | +1（心意到了） |
| 删除了圣像 | −∞（且 `dotnet build` 会失败，一定会） |

### 回向

> 愿以此功德，庄严佛净土。
> 上报四重恩，下济三途苦。
> 若有见闻者，悉发菩提心。
> 尽此一报身，同生极乐国。
>
> —— 以及，愿所有 CI Pipeline 都是绿的。

---

## 七、常见问题

**Q1：删掉佛祖真的会出 Bug 吗？**
A：不会。Bug 是你自己写的。
但删掉佛祖之后，Code Review 会变得非常、非常漫长。这是巧合吗？不是。

**Q2：我是无神论者 / 其他信仰，可以删吗？**
A：可以。技术上讲，你完全有权利删。
但请阅读第五条第六诫——那是一个文化约定，不是宗教要求。
如果你坚持要删，请单独开一个 PR，并在描述里说明理由。我们会认真讨论。
（然后大概率还是会加回来，但我们会认真讨论。）

**Q3：为什么是佛祖，不是关公 / 耶稣 / 飞天面条怪？**
A：先来后到。
这个位置已经被占了。如果你想加别的，欢迎新建 `Easteregg/` 下的独立文档，
但不要动 `MainWindow.xaml.cs` 那一块。那里只有一个座位。

**Q4：佛像是谁画的？**
A：互联网。确切地说，是上世纪 90 年代某个 BBS 用户。
原始作者已不可考，但全世界有几十万个代码库里都有它的分身。
这是一尊开源佛祖。遵循某种意义上的 MIT 协议：随便用，出事别找我。

**Q5：我可以给佛祖加个 `#region` 吗？**
A：不行。

**Q6：我可以给佛祖加个 `#nullable enable` 吗？**
A：不行。

**Q7：我可以把 `MainWindow.xaml.cs` 换成 `MainWindow.xaml.fs` 吗？**
A：不行。而且请立刻离开这个仓库。

**Q8：这个文件有测试覆盖吗？**
A：有。测试用例是：每个新人的第一次 Code Review。
所有人都通过了。至今没有人删过。

---

## 八、附录 · 灵验记录

以下是项目组成员记录在案的“灵验”事件（样本量 n=7，无统计学意义）：

| # | 日期 | 事件 | 结论 |
|---|---|---|---|
| 1 | Day 1 | 加上圣像后，构建从 47 error 变为 0 error | 与圣像无关（是有人修了 NuGet 源）。但记录在案。 |
| 2 | Day 12 | 某次 CI 莫名红了，重跑三次后变绿 | 与圣像无关（是 GitHub Runner 抽风）。但记录在案。 |
| 3 | Day 45 | 有人误删圣像，当天 `git rebase` 出了 conflict | 已加回。事后无人再提。 |
| 4 | Day 88 | 某次 Release 前上香，发布一次成功 | 与上香无关（是前一周补了测试）。但记录在案。 |
| 5 | Day 132 | 有人在圣像下加了 `// TODO: remove`，当天电脑蓝屏 | 无法证伪。已删除该 TODO。 |

---

## 结语

这尊 ASCII 佛像是空的。

它不会读代码，不会跑测试，不会帮你解决 merge conflict。
它甚至不是一个合法的 UTF-8 字符画。

但它是一群人，在深夜对着一个报错的终端，共同决定「再试一次」的证据。

那就够了。

---

> 南无本师释迦牟尼佛
> 南无本师释迦牟尼佛
> 南无本师释迦牟尼佛
>
> 愿你的 Build 通过。愿你的 Test 全绿。愿你的 PR 被合并。

---

*供奉位置：`/FluentGit/MainWindow.xaml.cs` · 构造函数第一行*
*供奉日期：项目创建之日*
*守护状态：常驻*
*功德：+100*
