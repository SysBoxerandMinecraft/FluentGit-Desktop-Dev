
# 🎋 Fortune Draw

> "Should I refactor today? — Ask the heavens."

Chinese version: [Fortune.md](./Fortune.md)

---

## 1. Origin

FluentGit has an ancient way of making decisions.

Not a meeting.
Not a vote.
Not an A/B test.

A fortune draw.

This tradition started during an architecture argument with myself.
I argued for two hours: “Should I split GitService right now?”
I could not convince myself.

So I said to myself:

“Let’s draw a fortune.”

The fortune came up Auspicious.
So I split it.
After the split, it really was cleaner.

Since then, whenever a technical decision is unclear, I draw a fortune.
A solo developer has no team vote. The fortune is the silent third party.

---

## 2. When Fortune Draw Applies

Fortune is not omnipotent.
Fortune **only answers questions where any choice would be fine**.

| Question Type | Fortune Allowed | Reason |
|---|:---:|---|
| Should I refactor now? | ✅ Yes | Both paths survive, just taste differently |
| Should I ship right now? | ❌ No | See First Prohibition in [`Taboos.md`](./Taboos.md) |
| Library A or Library B? | ✅ Yes | Both are solid, just different styles |
| Should I work overtime tonight? | ❌ No | The answer is always "no" |
| Should I add this feature? | ⚠️ Cautious | Check user feedback first, then draw |
| Fix the bug or write docs first? | ✅ Yes | Either is fine, depends on mood |
| Should I deploy on Friday? | ❌ No | See [`Taboos.md`](./Taboos.md) |
| Should I use `async void`? | ❌ No | The answer is always "no" |

**Principle:**
**If there is only one correct answer, do not draw.**
**If there are two correct answers, draw.**

---

## 3. The Fortune Chart

FluentGit has five fortune tiers. Each corresponds to a specific development decision.

### 3.1 The Supreme · 上上签

```text
╔═══════════════════════════════════════╗
║                                       ║
║          ✦  T H E  S U P R E M E  ✦  ║
║                                       ║
║         Today, move greatly.          ║
║         One move, a thousand years.   ║
║                                       ║
║     All requests answered.            ║
║     All paths open.                   ║
║                                       ║
╚═══════════════════════════════════════╝
```

**Interpretation:**
Today is the kind of day that comes **once in a decade**.
Architectures should be split however they should be split.
That refactor you have been putting off for three months — today it will flow.
That core class you have been afraid to touch — today you will have inspiration.

**But note: the Supreme appears very rarely.**
**If you draw it five times a month, your fortune bucket has a problem.**

**Recommended Actions:**
- Split architecture
- Refactor core modules
- Submit a large PR
- Thoroughly clean up `Utils.cs`

---

### 3.2 The Auspicious · 上签

```text
╔═══════════════════════════════════════╗
║                                       ║
║         ◆  T H E  A U S P I C I O U S ◆║
║                                       ║
║         Today, advance.               ║
║         Do not retreat.               ║
║                                       ║
║     Requests will be answered.        ║
║     Add a bit of caution.             ║
║                                       ║
╚═══════════════════════════════════════╝
```

**Interpretation:**
Today is suitable for **forward-progress** work.
Add a feature, write a test, fill in some docs.
Not suitable for tearing down and rebuilding.

**Recommended Actions:**
- Do a small refactor (split one class)
- Submit a feature PR
- Add a batch of unit tests
- Write a design document

---

### 3.3 The Neutral · 中签

```text
╔═══════════════════════════════════════╗
║                                       ║
║          ◇  T H E  N E U T R A L  ◇  ║
║                                       ║
║         Today, defend.                ║
║         Do not attack.                ║
║                                       ║
║     Requests are ordinary.            ║
║     No great joy, no great sorrow.    ║
║                                       ║
╚═══════════════════════════════════════╝
```

**Interpretation:**
Today, **do not move things around**.
It is neither bad nor good.
Suitable for **maintaining the status quo**, **fixing small bugs**, **reviewing others' PRs**.

**Recommended Actions:**
- Fix bugs
- Reply to issues
- Review someone else's PR
- Tidy up TODOs
- Tidy up folders
- Slacking off is also acceptable (but you will get caught)

---

### 3.4 The Inauspicious · 下签

```text
╔═══════════════════════════════════════╗
║                                       ║
║      ▲  T H E  I N A U S P I C I O U S ▲║
║                                       ║
║         Today, defend.                ║
║         Do not move.                  ║
║                                       ║
║     Requests are blocked.             ║
║     Be careful in all matters.        ║
║                                       ║
╚═══════════════════════════════════════╝
```

**Interpretation:**
Today, **change nothing**.
Not because you cannot, but because **anything you do today will go wrong**.

It may be your state of mind.
It may be the environment.
It may just be that kind of day.

**Recommended Actions:**
- Write documentation
- Write comments
- Organize the backlog
- Read a technical book
- **Absolutely do not**:
  - Modify core code
  - Split architecture
  - Ship a release
  - Update dependencies

---

### 3.5 The Ominous · 下下签

```text
╔═══════════════════════════════════════╗
║                                       ║
║         ☠  T H E  O M I N O U S  ☠   ║
║                                       ║
║         Today, go home.               ║
║         All matters are inauspicious. ║
║                                       ║
║     Requests unanswered.              ║
║     Pause everything.                 ║
║                                       ║
╚═══════════════════════════════════════╝
```

**Interpretation:**
Today is **hell mode**.

If you change code today, you will introduce bugs.
If you ship today, there will be an incident.
If you refactor today, you will tear down and rebuild three times.
If you push today, you will be reverted.

**This is not superstition. This is your own state.**

**Recommended Actions:**
- Take half a day off
- Or: only write documentation, no code
- Or: only review, no commit
- Or: re-read [`Incense.md`](./Incense.md) and burn another stick
- **Absolutely do not**:
  - `git push --force`
  - Operate in production
  - Deploy
  - Say "one last change"

---

## 4. The Fortune Code

Below is a minimal implementation of `FortuneService`.
Please **seriously** treat it as an engineering artifact.

```csharp
using System;
using System.Security.Cryptography;

namespace FluentGit.Services;

/// <summary>
/// Fortune draw service.
/// Does exactly one thing: returns a Fortune.
/// Uses cryptographically secure randomness, because decisions must not cheat.
/// </summary>
public static class FortuneService
{
    private static readonly Fortune[] Pool =
    {
        // Weight is proportional to rarity. The Supreme is rare, by design.
        Fortune.Supreme, Fortune.Supreme, Fortune.Supreme,

        Fortune.Auspicious, Fortune.Auspicious,
        Fortune.Auspicious, Fortune.Auspicious, Fortune.Auspicious,
        Fortune.Auspicious, Fortune.Auspicious, Fortune.Auspicious,
        Fortune.Auspicious, Fortune.Auspicious, Fortune.Auspicious,

        Fortune.Neutral, Fortune.Neutral, Fortune.Neutral, Fortune.Neutral,
        Fortune.Neutral, Fortune.Neutral, Fortune.Neutral, Fortune.Neutral,
        Fortune.Neutral, Fortune.Neutral, Fortune.Neutral, Fortune.Neutral,
        Fortune.Neutral, Fortune.Neutral, Fortune.Neutral, Fortune.Neutral,

        Fortune.Inauspicious, Fortune.Inauspicious, Fortune.Inauspicious,
        Fortune.Inauspicious, Fortune.Inauspicious, Fortune.Inauspicious,

        Fortune.Ominous, Fortune.Ominous
    };

    /// <summary>
    /// Draw a fortune.
    /// </summary>
    public static Fortune Draw()
    {
        // Do not use Random.Shared.
        // A fortune draw must not be pseudo-random.
        int index = RandomNumberGenerator.GetInt32(Pool.Length);
        return Pool[index];
    }

    /// <summary>
    /// Draw a fortune and log the result.
    /// </summary>
    public static Fortune DrawAndLog()
    {
        var fortune = Draw();
        AppLogger.Info("Fortune", $"Fortune drawn today: {fortune}");
        return fortune;
    }
}

public enum Fortune
{
    Supreme,       // 上上签
    Auspicious,    // 上签
    Neutral,       // 中签
    Inauspicious,  // 下签
    Ominous        // 下下签
}
```

### About the Random Number Generator

**Why use `RandomNumberGenerator` instead of `Random.Shared`?**

Because `Random.Shared` is **pseudo-random**.
Pseudo-random means: if you know the seed, you can predict the result.
If the fortune can be predicted, it is no longer a fortune.

**A fortune draw must not be pseudo-random.**
**A fortune draw must be truly uncertain.**

**This is the only place in the project where cryptographically secure randomness is enforced.**
**Because this is the only thing that cannot be cheated.**

---

## 5. Rules of the Draw

### 5.1 Only Once per Day

**Draw once every morning when you arrive at work.**
After drawing, **today follows that fortune**.

You may not draw again just because you got the Ominous.
You may not draw again just because the Neutral felt insufficient.
**One day, one draw. One draw, one day.**

### 5.2 No Regrets After Drawing

After drawing, **you must act according to the fortune**.

If you draw Inauspicious but there is an urgent bug to fix today —
**then fix it.**

Inauspicious does not mean "you cannot fix bugs".
Inauspicious means "**do not do extra things today**".
Fixing the bug is necessary, not extra.

**The fortune is a guide, not a law.**

### 5.3 Share the Fortune

You may post the fortune in the group chat.

Not as "I got Ominous today, so don't ask me for anything".
But as **letting the team know your state today**.

**Ominous is not a leave request.**
**Ominous is a reminder.**

### 5.4 Do Not Re-Draw on the Same Question

You may not draw again just because "the first Neutral wasn't satisfying enough".

**One question, one draw.**
**If you draw Neutral, it is Neutral.**

**Re-drawing = subconsciously wanting a specific answer = you already know the answer.**

---

## 6. The Seven Prohibitions

### First Prohibition · Do Not Lie About the Ominous

Do not, on a day you drew the Ominous, tell colleagues:

> "I drew the Supreme today, I'm in a great state."

**Do not deceive colleagues.**
**Also do not force yourself through an Ominous day.**

### Second Prohibition · Do Not Weight the Fortune

Do not modify the `Pool` array in `FortuneService` to make the Supreme more likely.

**The quality of the fortune bucket is its fairness.**

### Third Prohibition · Do Not Add a "Revive" Fortune

Do not add `Fortune.Retry` ("try again" fortune).

**There is no revive.**
**What the fortune is, is what it is.**

### Fourth Prohibition · Do Not Replace Decision-Making with Fortune Draw

Do not, during technical selection, bet **everything** on the draw.

**The purpose of the fortune draw is to break ties, not to replace thinking.**

```text
❌ "SQLite or LiteDB? Let's draw."
✅ "Both have been investigated, both meet requirements, both are mature.
    Let's draw."
```

**Drawing without investigation = laziness, not metaphysics.**

### Fifth Prohibition · Do Not Have Someone Else Draw for You

Do not ask a colleague to draw your fortune for you.

**The fortune is drawn for yourself.**
**Having another draw it means you do not want to face the result.**

### Sixth Prohibition · Do Not Cheat at the Draw

Do not:

- Peek into the bucket before drawing
- Intentionally shuffle in a way that leaves a certain fortune on top
- Draw again when unsatisfied

**The bucket knows.**
**Next time, it will give you the Ominous.**

### Seventh Prohibition · Do Not Publicize

Do not publicly claim:

> "The FluentGit team uses fortune draws to make technical decisions."

Official external statement:

> "FluentGit uses an intuition-based decision method built on engineer experience, for handling technical selections with equivalent alternatives."

As for what that "intuition" is —
**see Chapter 3 of this document.**

---

## 7. Merit & Dedication

| Action | Merit |
|---|---:|
| Drew the Supreme, then did a big refactor that day | +30 |
| Drew Auspicious, then advanced a feature that day | +15 |
| Drew Neutral, then held steady without moving things | +10 |
| Drew Inauspicious, then only wrote documentation | +20 |
| Drew Ominous, then took half a day off | +50 (self-care +40, honesty +10) |
| Drew Ominous, then forced a refactor anyway | −200 (see [`Taboos.md`](./Taboos.md)) |
| Drew Supreme, then did nothing | −30 (wasted luck) |
| Did not inform colleagues after drawing | 0 (personal choice) |
| Modified the fortune bucket weights | −500 |
| Lied to a colleague about the fortune tier | −100 |
| Had someone else draw for you | −50 |

### Dedication

> May this fortune,
> when Supreme, not make you proud.
> when Neutral, not make you restless.
> when Ominous, not make you despair.
>
> May today,
> whatever fortune you drew,
> you still leave work with a quiet mind.

---

## 8. FAQ

**Q1: Does fortune draw really work?**
A: Yes.
Its function is not to predict the future, but to **give your decision an external reason**.

When you are torn over "should I refactor now",
you are actually fighting yourself.
Draw a fortune. Let a third party (the fortune) decide for you.
**The decision burden shifts, and you relax.**

**Q2: Why five tiers?**
A: Because **five tiers correspond to five states**.
If only up/down, it is too crude.
If ten tiers, too complex.
Five is just right.

**Q3: Can I roll dice instead of drawing a fortune?**
A: No.
**Fortunes have culture. Dice do not.**
Dice produce numbers. Fortunes produce **meaning**.

**Q4: I drew the Ominous. Can I still write code today?**
A: Yes.
But **only documentation-level code**:

- Documentation
- Comments
- Logs
- Configuration
- Tests (if purely additive)

**Do not write business logic.**
**Do not modify core code.**

**Q5: I drew the Supreme. Am I guaranteed to succeed today?**
A: No.
The Supreme only indicates "today flows smoothly", not "today must win".
**You still have to do the work.**

**Q6: What if I draw the Ominous five days in a row?**
A: That means:

1. Your state is persistently bad — see a doctor.
2. Or, your fortune bucket is broken (theoretically impossible).
3. Or, you actually need several days of real rest.

**Ominous three days in a row is a signal.**

**Q7: Can I make my own fortune bucket?**
A: Yes.
Create a `PersonalFortune.md` under `Easteregg/`.
But do not modify this file.

**Q8: Why is the fortune service under `Services/`?**
A: Because it is a **Service**.
It serves **decisions**, not entertainment.
**Putting it in `Services/` is serious.**

---

## 9. Appendix · Real Fortune Draw Log

Real fortune draws recorded by the team (sample size n=23, for reference only):

| # | Question | Fortune | Outcome |
|---|---|---|---|
| 1 | Split `GitService` now? | Auspicious | Split. Cleaner. |
| 2 | Ship today? | Inauspicious | Did not ship. Shipped next day, first try success. |
| 3 | Add dark theme now? | Neutral | Added. Not hard. |
| 4 | Rewrite `RepoPage` now? | Ominous | Did not rewrite. Looked again next day — the original design was right. |
| 5 | Replace MVVM with MVU? | Inauspicious | Did not replace. Still haven't. Correct. |
| 6 | Add a batch of unit tests now? | Supreme | Added. Found two latent bugs. |
| 7 | Replace `Debug.WriteLine` with `AppLogger` now? | Neutral | Replaced. Smooth. |
| 8 | Upgrade .NET now? | Inauspicious | Did not upgrade. Waited two weeks, found a library incompatibility. |
| 9 | Delete `Utils.cs` now? | Supreme | Deleted. The whole team applauded. |

---

## Closing

Fortune drawing is not superstition.

**Fortune drawing is admitting — some decisions are right however you make them, and wrong however you make them.**
**Better to hand it to a silent third party.**

The fortunes in the bucket, we put there ourselves.
But the one we draw, we chose ourselves.

**We are not asking the fortune to decide for us.**
**We are asking the fortune to carry the weight of the decision for us.**

And then, regardless of the result,
**the next day, we draw again.**

---

> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
>
> One draw asks the heavens,
> one draw asks the earth,
> one draw asks the heart.
>
> May today's fortune,
> whether Supreme or Ominous,
> be what you need right now.

---

*Fortune bucket location: `FortuneService.cs` · Draw frequency: once per day*
*Guardian status: permanent*
*Merit: see Chapter 7*
