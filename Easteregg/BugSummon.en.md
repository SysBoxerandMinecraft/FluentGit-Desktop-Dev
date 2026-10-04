
# 🔮 Bug Summoning & Banishing Circle

> "If you cannot reproduce the bug, you have not recited the right incantation."

Chinese version: [BugSummon.md](./BugSummon.md)

---

## 1. Origin

I have two iron rules:

A bug that can be reproduced is a bug.

A bug that cannot be reproduced is a ghost.

Ghosts are troublesome.
They do not appear on my machine. They only appear on the user’s machine.
They do not appear in Debug. They only appear in Release.
They do not appear when I am staring at the screen. They appear in the 0.5 seconds it takes me to turn around and get water.

When facing a ghost, there are two approaches:

Scientific approach: Add logs, add telemetry, add traces, track it down, pin it.

Metaphysical approach: Draw a summoning circle.

This document covers both.
A solo developer has no team to help reproduce it, so the circle matters more.

---

## 2. The Three Forms of Bugs

Before discussing summoning circles, we must classify bugs.

| Type | Trait | Reproduction Rate | Approach |
|---|---|:---:|---|
| **Persistent** | Reproducible every time | 100% | Normal Debug |
| **Conditional** | Only under certain conditions | 30–70% | Add logs, narrow the condition |
| **Ghost** | Not reproducible; only on others' machines | 0% | **Summoning circle** |

**The ghost type is the only reason a summoning circle exists.**

If no one can reproduce it on their own machine, you can only approach it on the **mental level**.

---

## 3. The Circle Itself

### 3.1 Standard Summoning Circle (ASCII)

```text
                    ╔═══════════════════════════════╗
                    ║                               ║
                    ║        ☠  BUG  ☠             ║
                    ║                               ║
                    ║      ╱╲     ╱╲     ╱╲        ║
                    ║     ╱  ╲   ╱  ╲   ╱  ╲       ║
                    ║    ╱    ╲ ╱    ╲ ╱    ╲      ║
                    ║   ╱  A   ╳  B   ╳  C   ╲     ║
                    ║  ╱      ╱ ╲    ╱ ╲      ╲    ║
                    ║ ╱      ╱   ╲  ╱   ╲      ╲   ║
                    ║╱   D  ╳  E   ╳  F   ╳  G   ╲  ║
                    ║ ╲      ╲   ╱  ╲   ╱      ╱   ║
                    ║  ╲   H  ╳  I   ╳  J      ╱    ║
                    ║   ╲      ╲   ╱  ╲   ╱    ╱    ║
                    ║    ╲      ╲ ╱    ╲ ╱    ╱     ║
                    ║     ╲      ╲╱      ╱     ╱    ║
                    ║      ╲               ╱        ║
                    ║       ╲─────────────╱         ║
                    ║                               ║
                    ╚═══════════════════════════════╝

                          S U M M O N I N G
                             C I R C L E

                    〔 A 〕  Staging environment
                    〔 B 〕  Production environment
                    〔 C 〕  User A's machine
                    〔 D 〕  User B's machine
                    〔 E 〕  Windows 10 22H2
                    〔 F 〕  Windows 11 24H2
                    〔 G 〕  No-network environment
                    〔 H 〕  Low-memory environment
                    〔 I 〕  Cross-timezone scenario
                    〔 J 〕  Screen resolution 800x600
```

### 3.2 Circle Specifications

| Item | Spec |
|---|---|
| Material | Pure ASCII, no Unicode dependency (except box lines) |
| Size | 41 columns × 22 rows |
| Placement | Debug output window, or first comment block of `AppLogger.Error` |
| Activation | When encountering a non-reproducible bug |
| Duration | Until the bug reproduces. May be forever |
| Side effect | None. But do not stare for too long |

---

## 4. The Summoning Ritual

### 4.1 Preparation

**Before summoning, you must do four things.**

```text
[ ] Record the full bug report (verbatim, not paraphrased)
[ ] Record reproduction steps (even if it's "user said they clicked randomly")
[ ] Collect environment info (OS / .NET version / screen resolution / language)
[ ] Check whether the user is actually on the latest version
```

**Step four is the most important.**

**80% of ghost bugs are because the user is on an old version.**

Before you start drawing the circle, ask:

> "Can you confirm you are on v1.0.0?"

If they say "should be the latest" — **that is the problem.**

### 4.2 Draw the Circle

Open `AppLogger` and copy the full circle from Chapter 3 **verbatim** into the top of the log file.
**Do not omit any line.**

```csharp
namespace FluentGit.Services;

public static class AppLogger
{
    /*
     *                    ╔═══════════════════════════════╗
     *                    ║                               ║
     *                    ║        ☠  BUG  ☠             ║
     *                    ║                               ║
     *                    ║      ╱╲     ╱╲     ╱╲        ║
     *                    ... (truncated)
     *                    ╚═══════════════════════════════╝
     *
     *                          S U M M O N I N G
     *                             C I R C L E
     *
     *                    〔 A 〕  Staging environment
     *                    〔 B 〕  Production environment
     *                    ...
     */

    private const string TAG = "AppLogger";
    // ...
}
```

**Why draw it in the Logger?**

Because the Logger is the **only** place that runs in every environment.
It runs in Debug. It runs in Release.
It runs with UI. It runs without UI.

**The circle must be drawn where everyone passes through.**

### 4.3 Fill In the Circle

Open the circle and fill in each environment variable you **know**:

```text
〔 A 〕  Staging environment         → ✅ Tested, no issue
〔 B 〕  Production environment      → ⚠️ Untested
〔 C 〕  User A's machine            → ❓ Unknown
〔 D 〕  User B's machine            → ❓ Unknown
〔 E 〕  Windows 10 22H2             → ✅ Tested
〔 F 〕  Windows 11 24H2             → ⚠️ Partially tested
〔 G 〕  No-network environment      → ❌ Untested
〔 H 〕  Low-memory environment      → ❌ Untested
〔 I 〕  Cross-timezone scenario     → ❌ Untested
〔 J 〕  Screen resolution 800x600   → ❌ Untested
```

**See it?**

The cells you have not tested are where the ghost may appear.

**The circle will not fix the bug for you.**
**It will only tell you — what you have not tested.**
**Then you will go test it.**

**That is the entire secret of the summoning circle.**

### 4.4 Activation

After drawing and filling in the circle, add one line to `AppLogger.Error`:

```csharp
AppLogger.Error(TAG, "Summoning circle in place. Beginning the ghost hunt.");
```

Then, **start adding logs**.

The circle is not the end.
**The circle is the beginning.**

---

## 5. The Banishing Circle

If the summoning circle is to **find** a bug,
the banishing circle is to **drive it away**.

### 5.1 Standard Banishing Circle (ASCII)

```text
                ╔═══════════════════════════════════════╗
                ║                                       ║
                ║              ✟  B E G O N E  ✟      ║
                ║                                       ║
                ║         ～～～～～～～～～～～           ║
                ║        ～～～～～～～～～～～～         ║
                ║       ～～～～～～～～～～～～～        ║
                ║      ～～  ✦  BUG  ✦  ～～            ║
                ║     ～～～～～～～～～～～～～～         ║
                ║    ～～～～～～～～～～～～～～～        ║
                ║   ～～～～～～～～～～～～～～～～       ║
                ║                                       ║
                ║        Leave this process, now        ║
                ║              从此处，退出本进程         ║
                ║                                       ║
                ╚═══════════════════════════════════════╝

                          B A N I S H I N G
                             C I R C L E
```

### 5.2 When to Use the Banishing Circle

**The banishing circle cannot be used on real bugs.**
A real bug is a problem you must fix, not drive away.

**The banishing circle is used here:**

| Scenario | Suitable for Banishing |
|---|:---:|
| Known cause, fixed, awaiting verification | ❌ No (should close the bug record) |
| Third-party library bug, unfixable | ✅ Yes |
| A "suspected bug" that only exists in your head | ✅ Yes |
| A bug you investigated for three hours and decided not to pursue | ✅ Yes |
| A bug the user says "happens occasionally" and you have decided to accept | ✅ Yes |

**The purpose of the banishing circle is to give unfixed bugs a formal, honest, auditable resting place.**

---

## 6. Sealing a Bug in `#if false`

If a bug is too large to fix this iteration, but cannot be left unaddressed,
the team has a formal practice: **sealing.**

```csharp
#if false
// ╔═══════════════════════════════════════════════════════════╗
// ║                  ✦  SEALED  ✦                             ║
// ║                                                           ║
// ║  Bug ID      : #742                                       ║
// ║  Title       : Mica backdrop occasionally flashes white   ║
// ║                on Win10 19045                             ║
// ║  Reproduce   : Cannot reproduce reliably                  ║
// ║  Environment : Win10 19045, 16GB RAM, no discrete GPU     ║
// ║  Reported at : 2025-01-12                                 ║
// ║  Sealed at   : 2025-02-03                                 ║
// ║  Sealed by   : @username                                  ║
// ║  Reason      : Small impact, priority below v1.2 P0 items ║
// ║  Revisit at  : v1.3 or on additional reports              ║
// ║                                                           ║
// ║  ⚠️ This block is a seal, not a discard.                  ║
// ║  ⚠️ No one may delete it unless re-evaluating priority.   ║
// ╚═══════════════════════════════════════════════════════════╝

// Original logic (temporarily sealed):
// private void ApplyBackdrop() { ... }
#endif
```

### Rules of Sealing

1. **Must include a Bug ID.** A seal without an ID becomes a ghost in six months.
2. **Must document reproduction conditions.** Even if only "occasionally" — write it.
3. **Must state who sealed it.** If there is a problem, find them.
4. **Must state when to revisit.** Usually the next major version.
5. **Must note "do not delete".** Otherwise someone will clean it up as junk.

**Sealing is not avoidance. Sealing is honesty.**

---

## 7. The Seven Prohibitions

### First Prohibition · Do Not Send Real Bugs to the Banishing Circle

A real bug is a problem, not a ghost.
Sending it to the banishing circle is the same as "pretending you did not see it".
**Six months later, it will come back with three friends.**

### Second Prohibition · Do Not Treat a Seal as a Grave

Sealed code is **temporarily** preserved.
`#if false` is not a comment. It is **hibernation**.

If you open the `#if false` two years later and it is still identical —
**the seal has failed.**

### Third Prohibition · Do Not Seal Without an ID

Every seal must have a traceable ID (issue number / bug number / internal reference).
**A seal without an ID is a black hole.**

### Fourth Prohibition · Do Not Replace Logs with a Circle

The circle is the **starting point**, not the **ending point**.
After drawing the circle, add logs, add telemetry, add traces.
**Whoever draws the circle only in their mind will still be investigating the same bug a year later.**

### Fifth Prohibition · Do Not Draw the Circle in Front of Users

**The summoning circle is an internal ritual.**

Do not paste a summoning circle under a user's bug report.
That is not solving their problem. That is performance.

**Official external statement:**

> "We are tracking this issue in our reproduction environment. Thank you for your feedback."

### Sixth Prohibition · Do Not Skip Environment Verification

**80% of ghost bugs are environment mismatches.**

Before you draw the circle, do an environment comparison:

```powershell
# Information the user side must provide
$PSVersionTable
dotnet --list-sdks
dotnet --list-runtimes
[System.Environment]::OSVersion
Get-ComputerInfo | Select-Object OsName, OsVersion, CsTotalPhysicalMemory
```

**Match the environment, and half the bugs disappear.**

### Seventh Prohibition · Do Not Publicize

Do not publicly claim:

> "The FluentGit team draws summoning circles to investigate bugs."

Official external statement:

> "FluentGit uses a systematic environment matrix testing approach, covering multiple versions, resolutions, and language scenarios."

As for what that "environment matrix" is —
**see Chapter 3 of this document. It is, indeed, a matrix diagram.**

---

## 8. Merit & Dedication

| Action | Merit |
|---|---:|
| Drawing a full summoning circle | −10 (expends mental energy) |
| Drawing the circle, then adding logs | +30 |
| Drawing the circle, then discovering an untested environment | +50 |
| Successfully reproducing a ghost bug | +100 |
| Formally sealing an unreproducible bug (with ID) | +20 |
| Sending a real bug to the banishing circle | −100 |
| Sealing without an ID | −50 |
| Pasting a summoning circle in a user's issue | −200 |
| Revisiting a seal six months later and making a decision | +40 |
| Revisiting a seal six months later and closing it | −20 |

### Dedication

> May this summoning circle
> find the cause,
> find the environment,
> find the step you missed.
>
> May your bugs
> either be reproduced or be sealed.
> Not hang in the air and keep you awake.

---

## 9. FAQ

**Q1: Does the summoning circle really work?**
A: Yes.
Not because it changes anything, but because it forces you to **list every environment you know**.
**Once you list them, you will find: I actually tested nothing.**

**Q2: Can I skip the circle and just fix it?**
A: No.
If you do not even know what the bug is, fix what?

**Q3: Can I draw the summoning circle in `README.md`?**
A: No.
The summoning circle is an internal ritual, not promotional material.

**Q4: How long can a seal last?**
A: **One major version cycle.**
A seal that survives two major versions means it should not have been sealed — it should be implemented or deleted.

**Q5: What if I find a second bug inside a seal?**
A: **Open a new issue.**
Do not stack seals. Stacked seals = code archaeology.

**Q6: Can the summoning and banishing circles be drawn at the same time?**
A: No.
A project that simultaneously summons and banishes has an inconsistent internal attitude toward bugs.

**Q7: If a bug has not reproduced in a year, can it be auto-closed?**
A: It cannot be auto-closed.
**But it should be marked "dormant".**
A dormant bug, if reported again six months later, will have its priority **doubled**.

**Q8: Can I show this document to users?**
A: Yes.
But after reading it, users will most likely stop reporting bugs.

---

## Closing

The bug summoning and banishing circles are not real.

**But the thing behind them is real:**

**We do not know where the bug is.**
**We know that we do not know.**
**So we begin eliminating, one by one.**

**The summoning circle is an honest posture.**

Draw a circle. Write down what you know.
Leave the unknowns.
Then, test them one by one.

**Until the ghost shows its face from within the circle.**

---

> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
>
> May your bugs be reproducible.
> May your logs not lie.
> May your seals have IDs.

---

*Summoning circle location: `AppLogger.cs` or the debug output window*
*Banishing circle location: inside `#if false`, or under the `WontFix` label of Issues*
*Guardian status: briefly active only when needed*
*Merit: see Chapter 8*
