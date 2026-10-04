# 🚫 Developer Taboos

> "Some things, if you don't do them, will never go wrong. If you do them, they always do."

Chinese version: [Taboos.md](./Taboos.md)

---

## 1. Origin

FluentGit has not shipped a package yet.
This Taboo List comes from incidents in another project I once worked on.

It did not come from nowhere.
Under every taboo lies a real incident.

I did not want to write this document at first.
I thought, “I am an experienced engineer. I don’t need rules like these.”

Until I shipped a release on Friday afternoon in that project.
Until I said, “This change is tiny.”
Until I pressed F5 in production.

That night, I worked alone until 3 a.m.
The next morning, this document existed.

A solo developer has no colleague to clean up the scene.
So these taboos are my colleague.

---

## 2. The Twelve Taboos at a Glance

The following twelve are the consensus the FluentGit team bought with **overtime hours**.
Each one corresponds to a night that actually happened.

| # | Taboo | Violation Cost (Merit) |
|:---:|---|---:|
| 1 | Do not deploy on Friday afternoon | −500 |
| 2 | Do not say "this change is tiny" | −100 |
| 3 | Do not say "should be fine" | −80 |
| 4 | Do not say "works on my machine" | −100 |
| 5 | Do not say "this is the last change" | −200 |
| 6 | Do not `--force` push to shared branches | −500 |
| 7 | Do not refactor the day before a deadline | −200 |
| 8 | Do not merge without reading the diff | −150 |
| 9 | Do not treat TODO as a permanent solution | −50 |
| 10 | Do not debug in production | −500 |
| 11 | Do not trash ex-colleagues in code comments | −30 |
| 12 | Do not make big decisions at 3 a.m. | −300 |

> Violators face no actual punishment.
> But three months from now, at some late hour, you will remember what you said.

---

## 3. The Twelve Taboos Explained

### First Taboo · Do Not Deploy on Friday Afternoon

**The Story**

At 4:47 p.m. on a Friday in 2024, someone ran `dotnet publish -c Release`.
Five minutes later, the MSIX package was live.
Seven minutes later, users reported errors.
Nine minutes later, everyone started calling.

**No one could fix it.**

Because everyone was already on the way home.

**The Right Way**

```text
Allowed on Friday:
  ✅ Write code
  ✅ Write tests
  ✅ Write documentation
  ✅ Code Review
  ✅ Plan next Monday's work

Not allowed on Friday:
  ❌ git push --force
  ❌ dotnet publish
  ❌ Change production config
  ❌ Change database schema
  ❌ Change CI scripts
  ❌ Ship a release
```

**If You Already Did It**

1. Admit it in the group chat immediately.
2. Roll back immediately.
3. Apologize to your colleagues immediately.
4. Next Monday, re-run the three-incense ritual from [`Incense.en.md`](./Incense.en.md).

---

### Second Taboo · Do Not Say "This Change Is Tiny"

**The Story**

One sentence. Three words. In quotes.
It appeared seven times in the project. **All seven times it turned into something huge.**

The most extreme case: a colleague said, "Just one line of code."
That one line was in `App.xaml.cs`.
It exposed an incomplete MVVM architecture.
It took two weeks to fix.

**The Right Way**

Do not describe a change by its "size".
Describe it by its **scope**:

```text
❌ "This change is tiny"
✅ "This change touches 3 files, 1 interface, 0 DB migrations"

❌ "Should be quick to fix"
✅ "Estimated 30 minutes. If something unexpected comes up, I'll report early"
```

**If You Already Did It**

If you said "this change is tiny" and it turned out to be huge —
voluntarily say in the group chat:

> "I underestimated the scope. It's more complex than expected. I'll own it."

Merit −100, but attitude +50.

---

### Third Taboo · Do Not Say "Should Be Fine"

**The Story**

"Should be fine" is the **universal overture to every incident**.

- "This branch should be fine." — Result: conflict.
- "This parameter should be fine." — Result: out of range.
- "This time should be fine." — Result: timezone issue.
- "This user should be fine." — Result: a boundary value.

**The Right Way**

Delete "should". Either say "I tested it" or say "I didn't test it".

```text
❌ "Should be fine"
✅ "I tested locally. Scenarios A / B / C pass"

❌ "Probably fine"
✅ "Not tested. Want to go through it together now?"
```

**If You Already Did It**

Take what you said and rewrite it as a **verifiable statement**.

Confidence that cannot be verified is a blank check in the world of code.

---

### Fourth Taboo · Do Not Say "Works on My Machine"

**The Story**

This sentence has been passed around the programmer community for thirty years.
It is the **epitaph of cross-platform software engineering**.

In the FluentGit project, it appeared after a Mica backdrop rendering failure.
A colleague said, "Works on my machine."
Someone asked, "Is your machine Windows 11?"
The colleague fell silent.

**The Right Way**

```text
❌ "Works on my machine"
✅ "My machine is Windows 11 22H2, .NET 10.0.1. What's your environment?"

❌ "Must be your environment issue"
✅ "Let's diff our environment variables and SDK versions first"
```

"Works on my machine" is not wrong by itself.
**What's wrong is treating it as a conclusion, not a starting point.**

**If You Already Did It**

Rewrite the sentence as:

> "Works on my machine. Let's compare environments."

One extra sentence, and the merit comes back.

---

### Fifth Taboo · Do Not Say "This Is the Last Change"

**The Story**

This sentence is a **self-curse**.

The moment it leaves your mouth, the universe will schedule a new requirement for you.
And that requirement **must** be done before release.

The FluentGit team has officially blacklisted this sentence.
Whoever says it has to send a red envelope in the group chat.

**The Right Way**

```text
❌ "One last change and we ship"
✅ "All planned changes for this release are complete. New items will require a time re-estimate."

❌ "Just this one"
✅ "I'm starting the release process now."
```

**If You Already Did It**

If you said "this is the last change" and it **really was** the last one —
buy a coffee for the next person who gets burned by this sentence.

Because if it wasn't you, it will be them.

---

### Sixth Taboo · Do Not `--force` Push to Shared Branches

**The Story**

`git push --force` is a **legal, dangerous, tempting** command.

It is like a loaded gun.
In your own home (your own branch), it's fine.
In public (`main` / `develop` / `release/*`), **no**.

Someone once force-pushed to `main` in the FluentGit project.
Five minutes later, three people's work was gone.
Two hours later, it was recovered from reflog.
**But the psychological scar never fully healed.**

**The Right Way**

```bash
# Allowed
git push --force-with-lease origin my-feature-branch

# Absolutely forbidden
git push --force origin main
git push --force origin develop
git push --force origin release/*
```

If you really need to undo something on a shared branch, use `git revert`.
**Revert is for others to see. Force is for yourself to feel good.**

**If You Already Did It**

If you force-pushed to a shared branch:

1. Say it in the group chat immediately.
2. Post the reflog recovery instructions immediately.
3. Proactively ask a Reviewer to check for lost work.
4. Remove `--force` from your local aliases afterward.

---

### Seventh Taboo · Do Not Refactor the Day Before a Deadline

**The Story**

Refactoring is a good thing.
**But refactoring the day before a deadline is a bet.**

Win, and you're a genius.
Lose, and you're "the one who delayed the release by two weeks".

The FluentGit team has ruled:
**Within 7 days of a release, only Bug Fixes allowed. No Refactoring.**

**The Right Way**

```text
Allowed before a deadline:
  ✅ Fix bugs
  ✅ Add tests
  ✅ Write docs
  ✅ Polish comments
  ✅ Adjust logging

Not allowed before a deadline:
  ❌ Swap DI container
  ❌ Swap MVVM framework
  ❌ Rename core classes
  ❌ Split GitService
  ❌ Change async void to async Task (unless fixing a bug)
```

**If You Already Did It**

If you refactored the day before a deadline and broke it:

**Revert immediately.**

Do not try "one more small change".
That is the Fifth Taboo.

---

### Eighth Taboo · Do Not Merge Without Reading the Diff

**The Story**

"LGTM" is the cheapest sentence in Code Review.
**And the most expensive one.**

Because it once cost the FluentGit team a weekend.

A PR had 47 changed files. A reviewer glanced at the title, replied LGTM.
After merging, it turned out that one file had broken the initialization order in `AppState.cs`.
Merged on Friday. Discovered on Monday.

**The Right Way**

```text
Minimum review requirements:
  [ ] Read the full diff
  [ ] Leave at least one comment (even "no issue here, confirmed")
  [ ] Pull and run locally (at least one build)
  [ ] Confirm no unexpected files (bin/, obj/, logs, secrets)

Advanced review requirements:
  [ ] Understand the intent
  [ ] Consider edge cases
  [ ] Consider rollback plan
```

**If You Already Did It**

If you LGTM'd a PR you did not actually read:

1. Go read it now.
2. Proactively comment: "Adding what I missed in my earlier review."
3. Never LGTM like that again.

**The three letters LGTM must carry weight.**

---

### Ninth Taboo · Do Not Treat TODO as a Permanent Solution

**The Story**

TODOs have three fates:

1. **Implemented** — a minority.
2. **Deleted** — also a happy ending.
3. **Forgotten** — the majority.

The FluentGit team once cleaned up a batch of TODOs.
The oldest was written on the **third day** of the project:
`// TODO: optimize performance`

Three years later.
No one knows what to optimize.
No one dares delete it.
**It just hangs there, like a towel left out for three years.**

**The Right Way**

```csharp
// ❌ Bad TODO
// TODO: optimize later

// ❌ Bad TODO
// TODO: add tests

// ✅ Good TODO
// TODO(@username): When repo count > 5000, ReposPage loading takes 3 seconds.
//                  Plan to replace with virtualized list in v1.3 (see issue #142).

// ✅ Good TODO
// TODO(@username): Before 2025-Q2, convert GitService.Commit from sync to async.
```

**A good TODO needs:**
- An owner (`@username`)
- A trigger condition or deadline
- An action plan
- Ideally, an issue link

**If You Already Did It**

If you find yourself writing a "three-nothing" TODO:

**Fix it or delete it immediately.**

A TODO that is neither fixed nor deleted is not a TODO.
It is **mental pollution**.

---

### Tenth Taboo · Do Not Debug in Production

**The Story**

Production is a **dangerous place**.

It looks the same as your dev machine: a terminal, a Git repo, a `dotnet`.
**But every command you run there is real.**

Someone in the FluentGit team once ran on a production server:

```bash
git checkout main && git pull
```

To "update the code and see".
The pulled version was incompatible with the database schema at the time.
The service was down for 20 minutes.

**The Right Way**

```text
Allowed in production:
  ✅ View logs
  ✅ Check process status
  ✅ Check disk space
  ✅ Restart the service (if the action is clearly defined)
  ✅ Run pre-reviewed scripts

Not allowed in production:
  ❌ git pull
  ❌ dotnet build
  ❌ Manually edit config
  ❌ Manually edit the database
  ❌ "Try" any command
```

**If You Already Did It**

If you pressed F5 / Enter / Return in production:

**First thing: stop.**

**Second thing: say it in the group chat.**

**Third thing: write an incident report.**

Do not try to "fix it back" on your own.
Every extra action you take in production is **a new risk**.

---

### Eleventh Taboo · Do Not Trash Ex-Colleagues in Code Comments

**The Story**

Code comments are permanent.
**Every complaint you write will outlive your employment.**

The FluentGit project once found this inside a `Utils.cs`:

```csharp
// What kind of nonsense logic is this, whoever wrote it must be crazy
```

Later, `git blame` traced it to the original author.
**That original author is now the Tech Lead of this project.**

No one said anything.
But that comment was quietly deleted.

**The Right Way**

Code comments are for **explaining code**, not for **judging people**.

```csharp
// ❌ Bad comments
// What is this nonsense
// Whoever wrote this, come out
// Don't ask me, I don't know why it's written this way

// ✅ Good comments
// This logic exists to work around a Mica rendering bug on Win10 19045.
// When Mica is unavailable, SystemBackdrop must be set manually.
// See issue #87.
```

**If You Already Did It**

If you once trashed someone in a comment:

**Go fix the comment.**

Not because you fear being seen.
Because **that was never what comments are for**.

---

### Twelfth Taboo · Do Not Make Big Decisions at 3 A.M.

**The Story**

3 a.m. is a **dangerous hour**.

At that hour, you will feel:

- "This architecture should just be torn down and rebuilt."
- "I've actually wanted to quit for a long time."
- "Might as well swap the whole repo to another tech stack."
- "Would it really kill anyone to force push once?"

**Yes.**

The FluentGit team has ruled:
**After 1 a.m., no `git push --force`, no `rm -rf`, no architecture refactoring.**

**The Right Way**

At 3 a.m., only three things are allowed:

```text
1. Sleep.
2. Sleep.
3. Sleep.

If you really can't sleep:
- Write a journal entry
- Write TODOs
- Write a "things to do tomorrow" list

But do not:
- Write code
- Change architecture
- Ship a release
- Submit a PR
- Push to remote
```

**If You Already Did It**

If you made a big decision at 3 a.m.:

**Re-evaluate it the next morning.**

If you still think it's right then — **go ahead.**

If you think "what was I thinking last night" — **congratulations, you're back to normal.**

---

## 4. How the Taboos Relate

The twelve taboos are not isolated.

They are a net:
- Whoever breaks the Fifth Taboo ("last change") often also breaks the First (Friday deploy).
- Whoever breaks the Second ("tiny change") often also breaks the Seventh (refactor before deadline).
- Whoever breaks the Tenth (production debug) often also broke the Eighth (didn't read the diff).

**Why?**

Because behind them all is **the same mindset**:

> "It should be fine."

The twelve taboos, together, are really **one taboo**:
**Do not gamble.**

---

## 5. Merit & Dedication

| Action | Merit |
|---|---:|
| Following all twelve taboos for a full year | +1000 |
| Proactively admitting you broke one | +50 (honesty bonus) |
| Warning a colleague not to break one | +20 |
| Public apology in the group chat | +30 |
| Leaving early on Friday afternoon | +10 |
| Breaking the First Taboo (Friday deploy) | −500 |
| Breaking the Sixth Taboo (force push main) | −500 |
| Breaking the Tenth Taboo (production debug) | −500 |
| Breaking all twelve | Please consider resigning (joke) |

### Dedication

> May these twelve taboos
> guard your hands,
> guard your eyes,
> guard your nights.
>
> May you not ship on Friday,
> not refactor at midnight,
> not `git pull` in production.
>
> May every `git push` of yours
> never be a gamble.

---

## 6. FAQ

**Q1: Are these twelve hard rules?**
A: No. They are **consensus**.
Breaking them will not dock your pay, but reviewers will remember.
**Being remembered is worse than being docked.**

**Q2: What if my boss asks me to deploy on Friday afternoon?**
A: Deploy.
But do three things first:
1. Get the boss to sign off (not a joke — in writing).
2. Make sure everyone knows.
3. Write the rollback plan in advance.

**Boss's request = boss's responsibility.**

**Q3: What happens if I break one?**
A: Nothing.
**But three months from now, you'll wake up at midnight thinking about it.**

**Q4: If a colleague breaks one, should I call it out directly?**
A: Tell them privately.
Do not @ them in the group.
Do not screenshot and post it.
**Taboos are for reminding, not for shaming.**

**Q5: Can I add my own taboo?**
A: Yes.
Create a `PersonalTaboos.md` under `Easteregg/`.
But do not touch this file.

**Q6: Can I offset a violation with merit?**
A: No.
Merit can be offset. Incidents cannot.
**Code is not a bank. Bugs are not loans.**

**Q7: Why is there no taboo like "do not copy-paste code"?**
A: Because everyone copy-pastes.
Including the person writing this document.
**Taboos are written for everyone, including ourselves.**

**Q8: Will more taboos be added later?**
A: Yes.
Every incident may add one.
**May we never need a thirteenth.**

---

## Closing

Taboos are not meant to scare people.

Taboos are a **reminder card that a group of people wrote for you, using their overtime nights**.

It appears once before you press `Enter`.
It appears once before you run `git push --force`.
It appears once when you're about to make a decision at 3 a.m.

Whether you listen is up to you.

**But those overtime nights were real.**

---

> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
>
> May you break none of these.
> May you leave work on Friday without worry.
> May you be asleep at 3 a.m.

---

*Location of taboos: in the minds of every member of this project*
*Effective since: the day you signed on*
*Guardian status: permanent*
*Merit: gained by following, lost by breaking*