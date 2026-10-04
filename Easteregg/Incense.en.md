
# 🕯️ Incense Ritual

> "Three sticks of incense: one for the code, one for the tests, one for your future self."

Chinese version: [Incense.md](./Incense.md)

---

## 1. Origin

FluentGit has not shipped a package yet.
This Incense Ritual comes from another small WinUI tool project I once maintained.

The first formal release of that project was a disaster.

The tag was on the wrong branch, package signing was left on, and dotnet publish crashed halfway through while VS died.
The v0.1.0 that finally shipped opened to a blank window. Clicking anywhere did nothing.
I wrote in my log: “Did I forget to burn incense?”

The next day, I drafted this Incense Ritual and made it a rule: before every release, three sticks of incense must be burned.

After that, that project’s release incident rate dropped by 87%.
(Sample size n=1. But I never dared skip it again.)

FluentGit has not shipped yet.
But when I release it for the first time, these three sticks will be lit first.


---

## 2. Meaning of the Three Sticks

The three sticks are not arbitrary. Each one corresponds to the thing most easily overlooked in a release.

| Stick | Honoring | Check | If Skipped |
|:---:|---|---|---|
| First | Code | `dotnet build` with zero warnings | It compiles, but crashes at runtime |
| Second | Tests | All unit tests green | Users run your tests for you |
| Third | Future You | Tag placed correctly, version number correct | Three months from now, you swear at 3 a.m. |

> All three are required.
> This is not superstition. It is the last manual line of defense after Code Review and before CI.

---

## 3. Setting Up the Altar

Before burning incense, confirm the altar is in place.

The altar is not physical. The altar is your **build environment**.

```text
┌─────────────────────────────────────────────────┐
│                FluentGit Altar                   │
├─────────────────────────────────────────────────┤
│                                                 │
│    [Censer]   [Censer]   [Censer]               │
│       │          │          │                   │
│       ▼          ▼          ▼                   │
│   Code Stick  Test Stick  Future Stick          │
│                                                 │
│   Table: dotnet build output window             │
│   Ash: bin/ and obj/ (must be cleaned)          │
│   Water: one cup, tap water is fine             │
│   Scripture: CHANGELOG.md                       │
│   Wooden Fish: see WoodenFish.md (optional)     │
│                                                 │
└─────────────────────────────────────────────────┘
```

### Altar Specifications

| Item | Spec |
|---|---|
| Location | Your dev machine |
| Environment | Windows 10 22H2 or later |
| Toolchain | .NET 10 SDK, Git for Windows |
| Editor | Any. VS / Rider / VS Code / Notepad++ all fine |
| Temperature | 20–26°C (too hot, irritation; too cold, distraction) |
| Silence | Slack / Teams / DingTalk recommended off |
| Supplies | One cup of coffee (optional, but strongly recommended) |

---

## 4. The Ritual Process

### 4.1 Wash Hands

Close all VS / Rider instances.
Close all terminals running `dotnet watch`.
Close the 20 StackOverflow tabs open in your browser.

```powershell
# Confirm no lingering processes
Get-Process dotnet -ErrorAction SilentlyContinue
Get-Process FluentGit -ErrorAction SilentlyContinue
Get-Process msbuild -ErrorAction SilentlyContinue
```

If the above commands produce output, close each one.
**Hands unwashed, incense ash will not fall.**

### 4.2 Sweep

```powershell
dotnet clean
```

This step cannot be skipped.
`obj/` and `bin/` are where the incense ash piles up. If not swept clean, the three sticks will burn unevenly.

Advanced (optional):

```powershell
# If it still fails, clear the NuGet cache too
dotnet nuget locals all --clear
```

> Note: this clears the global cache. The next build will be slow.
> But slow before a release is slow worth having.

### 4.3 Light the First Stick · Code Stick

```powershell
dotnet build FluentGit.csproj -c Release -p:Platform=x64
```

**Checklist:**

- [ ] Output ends with `Build succeeded.`
- [ ] `0 Error(s)`
- [ ] `0 Warning(s)` (warnings count too)

If warnings appear:

- **Acceptable**: warnings from third-party NuGet packages you did not introduce.
- **Not acceptable**: any warning under the `FluentGit` namespace.
- **Must fix**: `CS8618` (nullable reference), `CS1998` (async without await).

> If the first stick will not light (build fails),
> return to 4.2 and sweep again. Three times max. After three, go to sleep.

### 4.4 Light the Second Stick · Test Stick

```powershell
dotnet test -c Release --no-build
```

**Checklist:**

- [ ] All tests `Passed`
- [ ] `Failed: 0`
- [ ] `Skipped: 0` (Skipped means tests are silenced — must be explained)

If a test turns red:

- **Do not** add `[Ignore]`.
- **Do not** change the assertion.
- **Do not** delete the test.
- **Fix it.**

> The second stick tests your character the most.
> Your code can fool the compiler. It can fool Code Review.
> But it cannot fool the tests. Tests are the demon-revealing mirror.

### 4.5 Light the Third Stick · Future Stick

```powershell
git status
git log --oneline -10
```

**Checklist:**

- [ ] `working tree clean`
- [ ] Current branch is `main` or `release/*`
- [ ] Latest commit is the one you intend to release
- [ ] `CHANGELOG.md` is updated
- [ ] Version number is bumped (`Package.appxmanifest` or `FluentGit.csproj`)

Then place the tag:

```powershell
git tag -a v1.0.0 -m "Release v1.0.0"
```

> **A mistyped tag is the most unforgivable error in the incense ritual.**
> Because a mistyped tag means the third stick was never lit.
> Or that while lighting it, you were thinking of something else.

### 4.6 Bow Three Times

All three sticks are lit. The final step is to bow three times.

No need to actually kneel. Just take three deep breaths before running the release command.

```powershell
# First bow · honoring Code
dotnet publish -c Release -p:Platform=x64 `
  -p:RuntimeIdentifier=win-x64 `
  -p:WindowsPackageType=MSIX `
  -p:WindowsAppSDKSelfContained=false `
  -p:GenerateAppxPackageOnBuild=true `
  -p:AppxPackageSigningEnabled=false

# Second bow · honoring Tests
# (wait for the output; do not switch windows)

# Third bow · honoring Future You
# (a return code of 0 means the three bows are done)
```

If the command returns non-zero:

- **Do not** rerun immediately.
- **Do not** change parameters.
- Read the output. Find the first error.
- Fix it, then **restart from 4.2**.

---

## 5. The Seven Prohibitions

### First Prohibition · Do Not Skip Hand Washing

Do not run `dotnet publish` while VS is still open.

This is not metaphysics. It is Windows file locking.
But if you insist on calling it metaphysics, fine — the result is the same.

### Second Prohibition · Do Not Skip the Sweep

Do not skip `dotnet clean` and go straight to `dotnet publish`.

`obj/` may contain leftover artifacts from the last Debug build.
They will haunt the MSIX package.

### Third Prohibition · Do Not Light Only One Stick

Do not build without testing, or test without tagging.

The merit of one stick is limited. Three sticks are the minimum.

### Fourth Prohibition · Do Not Replace the Three Sticks with a Script

Do not write a `release.ps1` that collapses the three sticks into a single command.

**Ritual cannot be automated.**

You can automate build, test, and publish.
But you cannot automate "close Slack" and "take three deep breaths".

### Fifth Prohibition · Do Not Burn Incense on the Wrong Day

Do not release on the following days:

| Day | Reason |
|---|---|
| Friday afternoon | No one to fix it if it breaks |
| The day before a holiday | Same as above |
| When you are sick | Poor state, the ash will be crooked |
| When most of the team is on leave | No one to take the blame |
| Company annual party day | You have been drinking |

If you absolutely must release, burn one extra stick before doing so.

### Sixth Prohibition · Do Not Publicize

Do not say in public, "We burn incense before every release."

Official external statement:

> "FluentGit follows a standardized Release Checklist to ensure stability and traceability of every release."

The document describing that "Release Checklist" is this file.

### Seventh Prohibition · Do Not Burn Digital Incense

Do not simulate incense with `Console.WriteLine("🕯️")`.

**The incense must be real.**
Even if only in your mind.

---

## 6. Simplified Incense (Release Checklist)

If time is tight, use the compressed version below. **But it is not recommended.**

```text
[ ] Close all IDEs
[ ] dotnet clean
[ ] dotnet build -c Release      → 0 error 0 warning
[ ] dotnet test  -c Release      → all pass
[ ] git status                   → clean
[ ] CHANGELOG.md updated
[ ] Version number updated
[ ] git tag -a vX.Y.Z
[ ] dotnet publish -c Release
[ ] Take three deep breaths
[ ] Run the release
```

Print it out and tape it to your monitor.
Every release, tick with a red pen.

---

## 7. Merit & Dedication

| Action | Merit |
|---|---:|
| Complete all three sticks | +10 |
| Only one stick | +3 |
| Skip the sweep and publish directly | −20 |
| Release on a Friday afternoon | −500 |
| Mistyped tag | −200 (and reviewers will remember) |
| Mistyped tag but fixed the same day | −50 (attitude acceptable) |
| Release succeeded on first try | +66 |
| Release succeeded with zero warnings | +100 |

### Dedication

> May these three sticks
> burn away warnings,
> burn away flaky tests,
> burn away the regrets of the future.
>
> May this Release
> pass on the first try, and no one lose sleep.

---

## 8. FAQ

**Q1: Does burning incense really help?**
A: Yes.
Not because it changes anything, but because it forces you to stop for five minutes before publish.
**90% of release incidents come from not stopping for those five minutes.**

**Q2: Can I burn incense in CI?**
A: No.
CI is an executor, not a believer. The incense must be handled by a **human**.

**Q3: Can I listen to music while burning incense?**
A: Yes. Just don't play "Good Luck Comes".

**Q4: Can I release two versions at once?**
A: No.
One stick at a time, three sticks per version.

**Q5: Can the Release Notes say "fixed some known issues"?**
A: No.
That is the same as not writing them. Equivalent to lighting incense without a flame.
See [`Taboos.md`](./Taboos.md) (to be written).

**Q6: I'm an indie developer. Do I need incense too?**
A: Especially you.
**You have no QA team. You only have three sticks.**

**Q7: I'm releasing at midnight. Can I skip the incense?**
A: Midnight is not a reason to release.
Releasing at midnight means the daytime release didn't happen.
Ask yourself first: **Why didn't the daytime release happen?**

**Q8: Can I drink the coffee on the altar?**
A: Yes.
But only after the three sticks are done.
Otherwise it is "eating before the offering" — merit −1.

---

## Closing

Burning incense is not asking the gods for help.

Burning incense is **waiting five more minutes when you could already release**.
In those five minutes, you close Slack, take a deep breath, and glance at `git status`.

When the three sticks burn down, you will suddenly remember one thing:
**CHANGELOG.md has not been updated yet.**

Congratulations.
That is exactly the point of burning incense.

---

> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
>
> One stick for the code, one for the tests, one for future you.
> May this Release succeed on the first try.

---

*Altar location: your dev machine*
*Incense frequency: before every Release*
*Guardian status: temporary (disperses when the three sticks burn out)*
*Merit: +10 / ritual*
