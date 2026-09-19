# 🧘 Digital Buddha

> **"The Buddha does not bless the code. The Buddha blesses the one who writes it."**

Chinese version: [Buddha.md](./Buddha.md)

---

## 1. Origin

On the first day of the FluentGit project, a Buddha moved into the constructor of `MainWindow.xaml.cs`.

It was an ordinary night. The first `dotnet build` reported 47 errors. Twelve came from NuGet restore failures.
Of the remaining 35, 34 were semicolon issues. The last one nobody ever figured out — it disappeared on its own.

The next morning, someone pasted an ASCII Buddha at the top of the `MainWindow.xaml.cs` constructor.
That afternoon, the build passed.

**There is no evidence these two events are connected.**
But since then, nobody has touched it.

---

## 2. The Icon

The current icon, enshrined at the top of the `MainWindow.xaml.cs` constructor in `s/FluentGit/`:

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
                Buddha bless        No BUG forever
```

### Specifications

| Item | Spec |
|---|---|
| Character set | Pure ASCII, no Unicode dependency, renders in any font |
| Width | ~55 columns, under the 80-column hard limit |
| Height | 20 lines |
| Encoding | UTF-8 without BOM |
| Indentation | Keep as-is. **Do not reformat.** |
| Dependencies | None. The Buddha depends on no NuGet package. |

---

## 3. The Offering

The actual enshrined form in `MainWindow.xaml.cs`:

```csharp
using FluentGit.Services;
using Microsoft.UI.Xaml.Media;   // ★ add this line
using FluentGit.Views;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
         *          _ooOoo_
         *         o8888888o
         *         88" . "88
         *         (| -_- |)
         *         O\  =  /O
         *      ____/`---'\____
         *    .'  \\|     |//  `.
         *   /  \\|||  :  |||//  \
         *  /  _||||| -:- |||||-  \
         *  |   | \\\  -  /// |   |
         *  | \_|  ''\---/''  |   |
         *  \  .-\__  `-`  ___/-. /
         * ___`. .'  /--.--\  `. . ___
         * ."" '<  `.___\_<|>_/___.'  >'"". 
         * | | :  `- \`.;`\ _ /`;.`/ - ` : | |
         * \  \ `-.   \_ __\ /__ _/   .-` /  /
         * ======`-.____`-.___\_____/___.-`____.-'======
         *                    `=---='
         * ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
         *               Buddha bless    No BUG forever
         */

        InitializeComponent();
        InitializeBackdrop();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string iconPath = Path.Combine(appDir, "Assets", "AppIcon.ico");
        AppWindow.SetIcon(iconPath);

        NavFrame.RequestedTheme = ElementTheme.Default;
        NavFrame.Navigate(typeof(RepoPage));
    }

    // Rest of the code (omitted here)
}
```

### About `InitializeComponent()`

The Buddha is before `InitializeComponent()`.

This is not an oversight. It is order. **First faith, then UI.**

---

## 4. Rituals

### 4.1 Daily

No action required. The Buddha is permanent. No daily check-in.
But if you're in a good mood, you may silently recite before committing:

```text
Build with no warnings, tests all pass, review with no comments.
```

### 4.2 Before Commit

```bash
git add .
git status          # Look at it. Actually look.
dotnet build        # Build before commit. That's the rule.
git commit -m "feat: xxx"
```

> If `dotnet build` fails, do not change code immediately.
> First `git stash`, then `git stash pop`, then build again.
> This is not metaphysics. It's NuGet cache. But the effect is the same.

### 4.3 Before Release

Before releasing a new version, perform the full three-incense ritual.
See [`Incense.md`](./Incense.md).

Brief version:

1. Close all VS / Rider instances.
2. Run `dotnet clean`.
3. Take a deep breath.
4. Run `dotnet publish -c Release`.
5. If it fails, go back to step 1. Max three times. After three, go to sleep.

### 4.4 For Newcomers

Before your first PR to FluentGit, run locally:

```bash
git log --oneline -- /FluentGit/MainWindow.xaml.cs | head -20
```

See how many people carefully worked around this comment.
**You will soon understand what they were protecting.**

---

## 5. The Seven Prohibitions

> These seven rules are the hard-earned consensus of the FluentGit team.
> Violators face no actual punishment, but reviewers will remember.

### First Prohibition · Do Not Delete

Do not delete the Buddha block in the `MainWindow.xaml.cs` constructor.

Even if you think it "takes up space", "is unprofessional", or "hurts readability".
**It does take up space. It is unprofessional. It must stay here.**

### Second Prohibition · Do Not Add "TODO"

Do not add comments like these above the Buddha:

```csharp
// TODO: remove this ASCII art before v1.0
// FIXME: what is this
// HACK: boss says can't delete, deal with it
```

The Buddha does not accept TODOs.

### Third Prohibition · Do Not Relocate

Do not extract the Buddha into `Constants.cs`, `Assets/`, `Resources/`, or any separate file.

**The Buddha does not move.**
The moment it moves to a separate file, it is just a string, no longer a Buddha.

### Fourth Prohibition · Do Not Blaspheme

Do not write disrespectful comments around the Buddha, such as:

```csharp
// What nonsense superstition (← forbidden)
// I'll delete it and see what happens (← forbidden, and something will happen)
// This code is meaningless (← technically correct, but unnecessary)
```

### Fifth Prohibition · Do Not Global Replace

Do not use `Ctrl + Shift + F` to search for "Buddha" and then "Replace All" or "Delete All".

**You may search. You may not replace.**

### Sixth Prohibition · Do Not Bypass Build

Do not skip `dotnet build` and go straight to `dotnet publish` in the release process.

This has nothing to do with the Buddha. It's pure engineering discipline.
It is here because someone once did this, and the tag was wrong.

### Seventh Prohibition · Do Not Publicize

Do not publicly claim that "FluentGit only compiles because of the Buddha".

**It can. But it shouldn't.**

Our official external statement is:

> "FluentGit is built with WinUI 3 and .NET 10, with a complete CI/CD pipeline and unit test coverage."

As for those few lines in `MainWindow.xaml.cs`, those are "developer comments". That's all.

---

## 6. Merit & Dedication

Enshrining the Buddha produces no merit. **Enshrining the Buddha produces a mindset.**

| Action | Merit |
|---|---:|
| Buddha permanently enshrined, never touched | +100 (project-level, permanent) |
| Reading this document | +3 |
| Copying the Buddha into your own project | +10 |
| Protecting the Buddha from deletion in a PR | +30 |
| Abandoning the idea to delete it after reading this | +50 |
| Writing "Buddha bless" in a commit message | +1 (intention counts) |
| Deleting the Buddha | −∞ (and `dotnet build` will fail. It will.) |

### Dedication

> May this merit adorn the Pure Land.
> Repay the four kindnesses above, relieve the three sufferings below.
> May all who see or hear awaken the Bodhi mind.
> May all, in this one life, be reborn in the Land of Bliss.
>
> — And may all CI pipelines be green.

---

## 7. FAQ

**Q1: Will deleting the Buddha actually cause bugs?**
A: No. Bugs are written by you.
But after deleting the Buddha, code review becomes very, very long. Coincidence? No.

**Q2: I'm an atheist / of another faith. Can I delete it?**
A: Yes. Technically, you have every right.
But please read the Sixth Prohibition — it is a **cultural agreement**, not a religious requirement.
If you insist on deleting, open a separate PR and explain why. We will discuss it seriously.
(Then probably add it back, but we will discuss it seriously.)

**Q3: Why Buddha, not Guan Yu / Jesus / Flying Spaghetti Monster?**
A: **First come, first served.**
This spot is taken. If you want to add others, create separate documents under `Easteregg/`.
But do not touch that block in `MainWindow.xaml.cs`. There is only one seat there.

**Q4: Who drew the Buddha?**
A: The Internet. Specifically, some BBS user in the 1990s.
The original author is unknown, but it exists in hundreds of thousands of codebases worldwide.
**This is an open-source Buddha.** Under a sort of MIT license: use freely, don't blame me.

**Q5: Can I add a `#region` around the Buddha?**
A: No.

**Q6: Can I add `#nullable enable` to the Buddha?**
A: No.

**Q7: Can I change `MainWindow.xaml.cs` to `MainWindow.xaml.fs`?**
A: No. And please leave this repository immediately.

**Q8: Is this file covered by tests?**
A: Yes. The test case is: **every newcomer's first code review.**
Everyone passed. No one has ever deleted it.

---

## 8. Appendix · Miracles

Recorded "miracles" (sample size n=7, statistically insignificant):

| # | Date | Event | Conclusion |
|---|---|---|---|
| 1 | Day 1 | After adding the Buddha, build went from 47 errors to 0 | Unrelated (someone fixed NuGet). But recorded. |
| 2 | Day 12 | CI turned red for no reason, reran three times and turned green | Unrelated (GitHub Runner hiccup). But recorded. |
| 3 | Day 45 | Someone deleted the Buddha, got a `git rebase` conflict that day | Restored. No one mentioned it again. |
| 4 | Day 88 | Burned incense before release, release succeeded first try | Unrelated (tests added the week before). But recorded. |
| 5 | Day 132 | Someone added `// TODO: remove` under the Buddha, blue screen that day | Unfalsifiable. TODO removed. |

---

## Closing

This ASCII Buddha is empty.

It does not read code, run tests, or resolve merge conflicts.
It is not even a valid UTF-8 character drawing.

But it is evidence that a group of people, late at night, staring at a failing terminal,
collectively decided: "**Just one more try.**"

That is enough.

---

> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
>
> May your build pass. May your tests be green. May your PR be merged.

---

*Enshrined at: `/FluentGit/MainWindow.xaml.cs` · first line of the constructor*
*Enshrined on: the day the project was created*
*Guardian status: permanent*
*Merit: +100*
