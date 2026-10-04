# 🪵 Digital Wooden Fish

> "Tap once, merit +1. Tap ten thousand times, the bug leaves on its own."

Chinese version: [WoodenFish.md](./WoodenFish.md)
Interactive version: [WoodenFish.en.html](./WoodenFish.en.html) (open directly in a browser)

---

## 1. Origin

I have a habit: after every self-review that tears me apart, tap the wooden fish once.

At first, only I tapped.
Later, still only I tapped.
Then I started putting an HTML wooden fish on screen during meetings, tapping while thinking.

It is not superstition. It is emotional regulation.

Self code review is where I get torn apart the most.
I am right. I am also right. But I am not talking about the same thing.
At that moment, tap the fish. Merit +1.

I did not win. But I did not explode. That is the merit.

---

## 2. The Fish Itself

FluentGit's digital wooden fish is a **standalone HTML file**, located at:

```
Easteregg/WoodenFish.html
```

Double-click to open. No build. No network. No CDN dependency.

It does exactly one thing:
**Tap it. Merit +1.**

### Fish Appearance (ASCII)

```text
        ___..___
     .-'        '-.
    /   _      _   \
   |   (_)    (_)   |
    \      __      /
     '-.______.-'
```

### Fish Specifications

| Item | Spec |
|---|---|
| Carrier | Single-file HTML |
| Dependencies | None. Pure vanilla HTML / CSS / JavaScript |
| Sound | Synthesized via Web Audio API, no external audio file |
| Theme | Follows system light / dark |
| Input | Mouse click, touch, spacebar |
| Counter | Purely local. Refresh clears it (this is impermanence) |
| Compatibility | Any modern browser: Edge / Chrome / Firefox / Safari |

---

## 3. The Knock Code

If you want to embed the wooden fish into the FluentGit main UI (we do not recommend it, but you can),
here is a minimal `WoodenFishService`:

```csharp
using System;

namespace FluentGit.Services;

/// <summary>
/// Digital wooden fish service.
/// Does exactly one thing: increments merit.
/// No locking, no persistence, no upload.
/// </summary>
public static class WoodenFishService
{
    private static long _merit = 0;

    public static long Merit => _merit;

    public static event Action<long>? OnKnock;

    public static void Knock()
    {
        _merit++;
        OnKnock?.Invoke(_merit);

        // At certain milestones, print a reminder
        if (_merit % 108 == 0)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[WoodenFish] Merit {_merit}. Go drink some water.");
        }
    }

    public static void Reset()
    {
        // Usually not needed. Merit cannot be cleared.
        // But if you insist, take responsibility.
        _merit = 0;
    }
}
```

Usage example:

```csharp
WoodenFishService.OnKnock += (n) =>
{
    // Update UI
    MeritTextBlock.Text = n.ToString();
};

WoodenFishService.Knock();
```

### About 108

**108** is a common number in Buddhist tradition.
Here, it is just a **randomly occurring gentle reminder**:

> You are tapping the fish. You have tapped 108 times.
> But in those 108 taps, you have not looked at the code once.
> Go look at the code.

---

## 4. The Knocking Ritual

### 4.1 When to Knock

| Moment | Taps |
|---|---|
| Code Review points out a real bug | 3 |
| Code Review points out a fake bug | 1 (but tap 100 in your heart) |
| Requirements changed again | 6 |
| Someone does `git reset --hard` in front of you | 9 |
| You see a `#region` with no comment | 1 |
| You see `async void` outside an event handler | 3 |
| You wrote a `// TODO` that is now three months old | 21 |
| You want to `git push --force` | 108. Really. Tap them all before deciding |

### 4.2 When Not to Knock

- When debugging a crash on a real device — **read the log first**.
- When your coding flow is going well — **do not interrupt**.
- When someone is explaining a technical problem to you — **let them finish**.

The wooden fish is a tool for comfort, not for avoidance.

### 4.3 What to Do After Knocking

Nothing.

**Really, nothing.**

Do not switch windows. Do not open the IDE. Do not reply to messages.
Just five seconds. Close your eyes. Breathe.

Then go back. Keep working.

---

## 5. The Seven Prohibitions

### First Prohibition · Do Not Rapid-Fire

Do not tap at 10 taps per second.
This is not merit, this is farming. Farming produces no merit, only misclicks.

### Second Prohibition · Do Not Automate

Do not write an AutoHotkey script to tap automatically.

**Merit cannot be outsourced.**

If you write a script, you are cheating.
If you cheat, the taps were wasted.

### Third Prohibition · Do Not Rank

Do not build a "merit leaderboard" inside the team.

This turns a good thing into a rat race.
**The end of the rat race is a bunch of people tapping at 3 a.m.**

### Fourth Prohibition · Do Not Trade Merit

Do not use any of the following:

- "I tapped 1000 times, can you review this PR for me?"
- "I have more merit, so listen to me."
- "I don't have enough merit, you go."

Merit is not negotiable.

### Fifth Prohibition · Do Not Make Wishes

Do not wish at the wooden fish, such as:

- "May this build pass."
- "May this bug reproduce."
- "May the PM not change requirements tonight."

**None of these are the fish's job.**
The fish handles only one thing: **you have tapped.**

### Sixth Prohibition · Do Not Publicize It as Faith

Do not say in public: "The FluentGit team believes in the digital wooden fish."

Official external statement:

> "The digital wooden fish is an internal emotional regulation tool for the team."

That is all.

### Seventh Prohibition · Do Not Advertise Merit

Do not write in commit messages:

```
fix: fixed a bug (merit +100)
```

Git log is for history. The wooden fish is for yourself.

---

## 6. Merit & Dedication

| Action | Merit |
|---|---:|
| Tap the wooden fish once | +1 |
| Stop at 108 and go look at the code | +10 |
| Tap instead of arguing when your code is criticized | +20 |
| Tap, then go fix that bug | +50 |
| Tap, then write a test | +80 |
| Automate with AHK | −100 |
| Build a merit leaderboard | −200 |
| Write "merit +100" in the PR description | −10 (and reviewers frown) |
| Embed `WoodenFish.html` in the main UI | −5 (aesthetic issue) |

### Dedication

> May this merit
> tap away anxiety,
> tap away restlessness,
> tap away tonight's overtime.
>
> After all the tapping, look back — there is code.

---

## 7. FAQ

**Q1: Does tapping really increase merit?**
A: No. Merit is fictional.
But after tapping, you will be **slightly calmer**. That is real.

**Q2: Why a wooden fish, not prayer beads, a prayer wheel, or incense?**
A: Because a wooden fish makes a sound.
**Things that make sound give feedback when tapped.**
Feedback makes you feel "I did something." That is its entire reason for existing.

**Q3: Can I change the styles in the HTML?**
A: Yes.
But please do not turn it into cyberpunk neon.
**The wooden fish is not meant to glow.** Really.

**Q4: Can I upload the merit count to a server?**
A: No.
**Merit is private.**

**Q5: I tapped, but the bug is still there. What now?**
A: Go read the code.
The fish helps you calm down. It does not help you debug.

**Q6: Can I share `WoodenFish.html` with friends?**
A: Yes.
But send `WoodenFish.md` along with it.
Otherwise they will not know what they are getting into.

**Q7: Does the HTML work offline?**
A: Yes.
It is a single file with no external links, no CDN.
**It works even with the network down.**

**Q8: Can I persist the tap count in `localStorage`?**
A: You could.
But please do not. **Refresh clearing it is a reminder of impermanence.**

---

## 8. Appendix · Knock Log

Real knock events recorded by team members (small sample, for reference only):

| # | Scenario | Taps | Outcome |
|---|---|---:|---|
| 1 | First Code Review torn apart | 12 | Calm. Fixed the code the next day. |
| 2 | Requirements changed the day before release | 36 | Calm. Release pushed back two days. |
| 3 | Someone force-pushed to main | 108 | Calm. Added a branch protection rule. |
| 4 | Found your own three-year-old code | 21 | Not calm. But fixed it. |
| 5 | Saw someone wrap sync method in `Task.Run` | 3 | Calm. Wrote a review comment. |
| 6 | Saw someone `Remove` inside `foreach` | 6 | Calm. Wrote a review comment. |
| 7 | Saw `async void` used in a service | 9 | Calm. Wrote two review comments. |

---

## Closing

The fish is fake. Merit is fake. The count clears on refresh.

**But the taps you made were real.**

You really stopped for a few seconds.
You really breathed a few times.
You really did not fire back immediately.

Then you went back and kept writing code.

That is the entire reason the digital wooden fish exists.

---

> Tap.
> Tap.
> Tap.
>
> Merit cannot be obtained.
> But the mind can be settled.

---

*Fish location: `Easteregg/WoodenFish.html`*
*Knock frequency: as you wish*
*Guardian status: permanent (as long as the browser is open)*
*Merit: +1 / tap*
