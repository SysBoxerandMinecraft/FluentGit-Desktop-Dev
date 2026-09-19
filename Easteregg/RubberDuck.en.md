
# 🦆 Rubber Duck Debugging

> "The duck cannot write code. But the duck makes you write the code yourself."

Chinese version: [RubberDuck.md](./RubberDuck.md)

---

## 1. Origin

On the desks of the FluentGit team sits a small yellow duck.

It might be plastic. It might be rubber. It might just be a yellow, round, seemingly harmless thing.
**But it is the quietest, most patient, most expensive engineer on this project.**

It does not speak.
It does not interrupt.
It does not say "I get it" when you are only halfway through.

It just looks at you.

**And then you find the problem yourself.**

---

## 2. What Is Rubber Duck Debugging

Rubber Duck Debugging is an **ancient, free, and severely underrated** debugging technique.

The process has only three steps:

```text
1. Get a duck (or anything that cannot talk back).
2. Explain your code to it, line by line.
3. Halfway through, you suddenly say: "Wait, that's not right."
```

**Step three is the only step that matters.**

The first two exist only to get you to the third.

---

## 3. Why the Duck Works

The duck does not solve your problem.
**The duck solves a different problem — the noise in your head.**

When you explain a problem to a colleague, your brain automatically starts social processing:

- "Should I say this?"
- "Will they think I'm bad at this?"
- "Should I set up some context first?"

This processing **eats up 20% of your thinking bandwidth**.

But the duck does not.
**The duck does not need context. The duck does not care if you're bad. The duck does not interrupt.**

So all your cognitive resources go into **understanding the problem itself**.

And then — you see it.

> The duck is a **cognitive bandwidth reclaimer**.

---

## 4. Duck Specifications

In the FluentGit team, the duck has formal specifications.

| Item | Spec |
|---|---|
| Material | Plastic / rubber / ceramic / cardboard / on-screen emoji, all acceptable |
| Color | Yellow (strongly recommended). Other colors require team approval |
| Height | 6–12 cm |
| Beak | Smiling |
| Eyes | Bigger is better (stronger gaze) |
| Position | Right side of the monitor, ~30 cm away |
| Orientation | Facing the user |
| Dependencies | None. The duck depends on no NuGet package |
| Name | **Must** have a name |

### About Naming

**A duck must have a name.**

Not "the duck", not "little duck", not "the yellow one".
**A specific name.**

For example:

- `Duck`
- `Quack`
- `Ducky`
- 小黄
- 鲁班
- Or anything that feels right to you

**A named duck appears to be listening.**
**A nameless duck is just a decoration.**

---

## 5. Daily Use of the Duck

### 5.1 Standard Flow

```text
1. Encounter a bug. More than 15 minutes with no clue.
2. Turn the duck to face you.
3. Start talking:

   "Duck, here's the thing. This function takes a repo path."
   "Then it calls GitService.GetStatus."
   "GetStatus runs git status --porcelain."
   "Then..."

   (pause for 3 seconds)

   "Wait."
   "I passed the absolute path of the repo."
   "But GitService expects a relative path."
   "So it's been running git status in the wrong directory this whole time."

   (3 minutes later, the bug is fixed)
```

**Throughout the entire process, the duck said nothing.**

### 5.2 When to Use the Duck

| Scenario | Duck Appropriate |
|---|:---:|
| A bug with no clue for over 15 minutes | ✅ Strongly recommended |
| A bug that has consumed 2 hours | ✅ Mandatory |
| A bug that has consumed a full day | ✅ Use the duck, but sleep first |
| Writing a new feature, fuzzy thinking | ✅ Recommended |
| Modifying code you don't understand | ✅ Strongly recommended |
| Just wanting to chat with the duck while slacking off | ❌ No |
| Wanting the duck to do Code Review for you | ❌ No (it cannot speak) |

### 5.3 When Not to Use the Duck

- **When the duck has stopped looking at you.** (You've been talking for 20 minutes.)
- **When you want the duck to apologize for you.** (Ducks lack social skills.)
- **When you are in a public meeting.** (Unless everyone in the team has one.)

---

## 6. Advanced Duck Techniques

### 6.1 Code Walking

Not just **talking**, but **walking**.

```text
1. Open the code.
2. With the mouse cursor, start from the first line and walk down one line at a time.
3. For every line, read it aloud to the duck.
4. Do not skip lines.
5. Do not skip functions.
```

**Steps 4 and 5 are the hardest.**

Because when you **skip a line**, you are **skipping the line that might be the problem**.

**The duck does not allow skipping.**

### 6.2 Pseudo Dialog

Some teams anthropomorphize the duck and have a "conversation".

```text
You: Duck, why does this code crash?
Duck: (silence)
You: Is it a null reference?
Duck: (silence)
You: No, wait. If it were null, the earlier guard would catch it.
Duck: (silence)
You: So it's a concurrency issue.
Duck: (silence)
You: No, not that either. Concurrency issues are intermittent. This one is deterministic.
Duck: (silence)
You: Ah.
```

**The duck is always silent.**
**But every silence pushes you toward the answer.**

### 6.3 Reverse Duck

If you've been talking to the duck for 30 minutes with no clue —
**switch ducks.**

Not the physical duck. Switch **mental positions**:

- Stand up. Walk to the other side of the room.
- Pretend to be **a different duck**.
- Explain the whole thing again from a completely opposite angle.

**The same story, with different ears, hears different problems.**

---

## 7. Ducks and Merit

The duck produces no merit.
**The duck produces only the moment of — "Oh, that's it."**

But the team still has classifications for duck-related behavior:

| Action | Merit |
|---|---:|
| Used the duck to solve a bug | +3 |
| Used the duck to solve in 15 minutes a 3-hour problem | +20 |
| The duck fell over while you were using it | 0 (pick it up and continue) |
| A colleague saw you using the duck | −1 (embarrassing) |
| Lending the duck to a colleague | +10 |
| Giving the duck away | −100 (you will lose it) |
| Naming the duck | +5 |
| Naming the duck more than three times | −10 (unfaithful) |
| Getting angry at the duck | −50 (it did nothing wrong) |
| Throwing the duck | −200 (and you will regret it) |

---

## 8. Ducks and AI

Someone asked: "Now that we have AI, do we still need the duck?"

**Yes. Even more so.**

Because AI **gives you an answer immediately**.
And the duck **makes you give yourself an answer immediately**.

These are not the same thing.

AI is an **answer machine**.
The duck is a **silence machine**.

**Sometimes, you need the latter.**

### When to Use AI

- You already know the problem, and you need a specific solution.
- You hit a general problem (regex, time formatting, C# syntax).
- You want to quickly try a few variations.

### When to Use the Duck

- You **do not know** what the problem is.
- You hit a problem **related to your own code**.
- You know what the problem is, but you **do not want to admit it**.

**That last one is where the duck is most valuable.**

---

## 9. The Seven Prohibitions

### First Prohibition · Do Not Blame the Duck

Do not say to the duck:

> "It's all your fault."

The duck did nothing.
The duck has not even spoken.
**Do not take it out on something that cannot argue back.**

### Second Prohibition · Do Not Use the Duck for Code Review

Do not write in a PR description:

> "Reviewed by Duck."

**The duck does not review.**
**The duck only makes you review.**

### Third Prohibition · Do Not Show Off the Duck

Do not write in a PR description:

> "Duck helped me find this bug."

**The duck wants quiet.**
**Do not draw attention to it.**

### Fourth Prohibition · Do Not Have Two Ducks

Do not place two ducks on the same desk.

**Two ducks will fight.**

Not physically. **Neither of them will listen to you properly.**
Because each is waiting for you to choose the other.

**Choose one. Use it forever.**

### Fifth Prohibition · Do Not Put the Duck on the Keyboard

Do not put the duck on the keyboard.

Not because it might press keys.
Because **it will block the line you need to read**.

### Sixth Prohibition · Do Not Write the Duck into Code

Do not write the duck into code, such as:

```csharp
// TODO: ask duck
// Duck bless
public class RubberDuckDebugger { }
```

**The duck lives outside the code.**
**The code does not need a duck.**

(Except this document, `Easteregg/RubberDuck.md`.)

### Seventh Prohibition · Do Not Publicize

Do not publicly claim:

> "The FluentGit team uses a plastic duck to debug code."

Official external statement:

> "The FluentGit team uses structured problem analysis and conducts verbal walkthroughs on difficult bugs."

As for **who the audience** of that walkthrough is —
**see Chapter 4 of this document.**

---

## 10. Duck Quotes

The following are real statements recorded by the team. The speaker is human. The duck is listening.

> "Duck, this function should return a list."
> "Duck, why is it empty?"
> "Duck, I clearly passed it in."
> "Duck, wait."
> "Duck, I passed the wrong thing."

> "Duck, I suspect a threading issue."
> "Duck, that's impossible, I already added a lock."
> "Duck, wait. I locked a different object."

> "Duck, this UI won't refresh."
> "Duck, I bound to an ObservableCollection."
> "Duck, I changed the collection."
> "Duck, wait. I changed the Collection, not the ObservableCollection."

> "Duck, why doesn't it enter this if branch?"
> "Duck, the condition is `status == 'Ready'`."
> "Duck, the passed value is also 'Ready'."
> "Duck, wait. It's a quote character issue."

> "Duck, I don't understand."
> "Duck, I understand nothing."
> "Duck, wait. I actually do understand."

---

## 11. Merit & Dedication

| Action | Merit |
|---|---:|
| Duck permanently on desk | +10 (project-level) |
| Using the duck at least once per week | +5 |
| Used the duck to solve a bug lasting over an hour | +30 |
| Used the duck to find your own stupid mistake | +50 (honesty bonus) |
| Lending the duck to someone | +10 |
| Losing the duck | −200 (it will not come back) |
| Getting angry at the duck | −50 |
| Letting the duck review for you | −100 |
| Getting filmed while using the duck | −10 (embarrassing) |

### Dedication

> May this duck
> quack once, and stop your racing thoughts.
> quack twice, and make you speak clearly.
> quack thrice, and let you find the answer yourself.
>
> May your next bug
> be exposed by the time you reach line seven.

---

## 12. FAQ

**Q1: Is there scientific evidence for the duck?**
A: Yes.
Its scientific basis is **Cognitive Load Theory**.
Put simply: **explaining to another forces you to reconstruct the problem, which reveals details you overlooked.**

**Q2: Can I use something other than a duck?**
A: Yes.
But it must satisfy three conditions:
1. **It does not speak.**
2. **It does not interrupt.**
3. **It does not judge you.**

Things that satisfy all three: a rubber duck, a figurine, a potted plant, a cat (unless it speaks), your own reflection.

**Q3: Can I use AI instead of the duck?**
A: It depends.
AI speaks, interrupts, and judges you (even if neutrally).
**The true duck is only silent.**
**When you are not sure what the problem is, silence is more valuable than an answer.**

**Q4: Do ducks have feelings?**
A: No.
But you will develop feelings for it.
**That is why it works.**

**Q5: Can I lend the duck to others?**
A: Yes.
But tell them: **talk to it, do not wish upon it.**

**Q6: What if my duck speaks?**
A: See a doctor.

**Q7: Can a duck be AI-generated?**
A: No.
The duck must be **physical, gazeable, and real**.
A duck on the screen gets covered by other windows.
**The duck cannot be `Alt + Tab`'d.**

**Q8: Why a duck, not a rubber bear, ceramic cat, or paper crane?**
A: Because the duck came first.
**First come, first served.**
The duck has been adopted by the programmer community for decades.
Bears, cats, and cranes are still waiting to be adopted.

---

## Closing

The duck is empty.

It will not answer, correct, or offer a better solution.
It just sits there, looking at you.

**But in the process of explaining to it, you will hear the sentence you did not say out loud.**

That is everything the duck is.

**A silent duck is closer to the truth than a hundred encouragements.**

---

> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
>
> The duck is listening.
> Keep talking.
> You're almost there.

---

*Duck location: right side of the monitor, ~30 cm away, facing the user*
*Duck name: decided by the owner*
*Guardian status: permanent, but it does not speak*
*Merit: see Chapter 11*
