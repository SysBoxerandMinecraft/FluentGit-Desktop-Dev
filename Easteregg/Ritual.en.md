
# 📜 Full Release Ritual

> "A release is not a task. A release is a ritual."

Chinese version: [Ritual.md](./Ritual.md)

---

## 1. Origin

FluentGit has not reached v1.0.0 yet.
This Full Release Ritual comes from v0.9.0 of another project I once worked on.

I did not prepare for the day that project’s v0.9.0 shipped.

It was an ordinary Wednesday afternoon.
CI was green. Tests passed. The code looked clean.
So I pressed dotnet publish -c Release.

Seven minutes later, v0.9.0 was live.
Nineteen minutes later, users started reporting issues.
Thirty minutes later, I discovered the database schema was not synchronized.
Forty-five minutes later, I noticed CHANGELOG.md still said v0.8.0.

That day’s lesson became this document.

FluentGit has not shipped a package yet.
But a solo developer has no release manager.
I am the release manager.
---

## 2. How the Ritual Relates to the Three Sticks

Someone asked: what is the difference between this document and [`Incense.md`](./Incense.md)?

**The three sticks are one step of the ritual.**
**This document is the ritual itself.**

```text
Incense.md        →  The five-minute incense burn before release
Ritual.md         →  The full process from T-7 to T+7
```

The three sticks are the **concentration** of ritual.
The full release ritual is the **unfolding** of ritual.

Both are needed.
The three sticks make you stop.
The full ritual keeps you from missing anything.

---

## 3. Ritual Overview

The FluentGit release ritual has six acts.

| Act | Time | Name | Core Task |
|:---:|:---:|---|---|
| First | T-7 | **Purification** | Freeze features, clean branches |
| Second | T-3 | **Preparation** | Prepare release notes, organize version numbers |
| Third | T-1 | **Recitation** | Read through CHANGELOG, recite the version declaration |
| Fourth | T-0 | **Action** | Three sticks of incense + release |
| Fifth | T+1 | **Verification** | Verify the release, monitor for anomalies |
| Sixth | T+7 | **Reflection** | Review, retrospect, dedicate |

**All six acts must be observed.**

Not because a ritual must be complete.
But because **each act corresponds to a real incident.**

---

## 4. First Act · Purification · T-7

### 4.1 Purify the Field

Starting from T-7, **new features are forbidden to merge**.

```powershell
# Freeze the main branch (publicized via PR description)
echo "🔒 Feature Freeze from $(Get-Date -Format 'yyyy-MM-dd') to vX.Y.Z release" `
  >> RELEASE_FREEZE.md
```

**Allowed to merge:**
- Bug fixes (must reference an issue)
- Test additions
- Documentation updates
- Localization (if already ready)

**Not allowed to merge:**
- New features
- Architecture changes
- Dependency upgrades (unless security fix)
- Class / file renames
- Public interface changes

### 4.2 Purify Branches

```powershell
# List all local branches
git branch -vv

# List all remote branches
git fetch --all --prune
git branch -r

# Delete merged local branches
git branch --merged main | ForEach-Object {
    if ($_ -notmatch '^\*|main|develop') {
        git branch -d $_.Trim()
    }
}
```

**The point of purification:**
Every extra branch before a release is one more chance for a mistake.

### 4.3 Purify the Mind

On T-7, **give yourself 15 minutes**.

Not looking at code. Not looking at issues. Not looking at Slack.
Looking at the sky outside the window.

**Because for the next seven days, you will be busy.**
**You need to remember why you started writing this thing in the first place.**

---

## 5. Second Act · Preparation · T-3

### 5.1 Prepare Release Notes

Open `CHANGELOG.md` and organize it in this format:

```markdown
## [v1.0.0] - 2025-03-15

### ✨ Added
- Branch switching feature (#112)
- Commit history view (#118)

### 🐛 Fixed
- Mica backdrop flashing white on Win10 19045 (#87)
- Parse error when repo path contains non-ASCII characters (#95)

### 🔧 Improved
- ~40% faster RepoPage loading (#101)
- Unified AppLogger log format (#108)

### 📝 Documentation
- Added Building.ps1 usage guide
- Updated Easteregg index

### ⚠️ Known Issues
- Cross-timezone repo times still slightly off; planned for v1.1 (#123)
```

**Rules:**
- **Every change must reference an issue number.** If it has no issue, it should not have been merged.
- **Do not write "fixed some issues".** See [`Taboos.md`](./Taboos.md).
- **Do not write "performance improvement".** Write a number.
- **Known issues must be disclosed.** Hiding known issues is release fraud.

### 5.2 Prepare Version Numbers

Update the version number in the following files:

```powershell
# 1. FluentGit.csproj
<Version>1.0.0</Version>

# 2. Package.appxmanifest
<Identity Version="1.0.0.0" />

# 3. CHANGELOG.md
## [v1.0.0] - 2025-03-15

# 4. Version badge in README.md (if any)
```

**Rules:**
- **The version number must match the tag.**
- **The version number must match the release notes.**
- **Once a version number is in CHANGELOG, it cannot be changed** — unless it was a typo.

### 5.3 Prepare the Rollback Plan

**Every release must have a rollback plan.**

```markdown
## Rollback Plan for v1.0.0

### If the MSIX package is broken:
1. Unpublish from Microsoft Store (if published)
2. Restore previous MSIX from backup
3. Open a post-mortem issue

### If there is a critical bug:
1. Decide: hotfix or rollback?
2. If hotfix: branch from v0.9.0, apply minimal fix, re-release as v1.0.1
3. If rollback: restore previous version, tag the bug as a v1.0.1 blocker

### Rollback Owner: @username
### Rollback Deadline: within 24 hours of discovery
```

**A release without a rollback plan is not a release.**
**It is a gamble.**

---

## 6. Third Act · Recitation · T-1

### 6.1 Read Through CHANGELOG

On T-1, **read `CHANGELOG.md` from start to finish**.

Not scan it. **Read it.**
With your mouth.

**You will find problems.**

- A feature description is wrong
- An issue number is wrong
- A version number is wrong
- One entry you no longer remember what it was

**The last one is the most dangerous.**

If you do not remember what that change was, it might be someone else's.
**You must figure it out.**

### 6.2 Recite the Version Declaration

**Every release has a declaration.**

The declaration for v1.0.0 is:

> "FluentGit, from today, can be used from scratch by someone who has never used it."

This is not marketing.
This is the **threshold for release**.

**If your version number is 1.0.0, it must be able to say this.**
**If it cannot, the version number should still be 0.x.**

### 6.3 Rehearse the Three Sticks

On the evening of T-1, **dry-run the three sticks once**.

Not an actual release, but **walking the whole process**:

```powershell
# 1. Close all IDEs
# 2. Run
dotnet clean
dotnet build -c Release -p:Platform=x64
dotnet test  -c Release --no-build
# 3. Check git status
git status
# 4. Fake tag (local only, do not push)
git tag -a v1.0.0-test -m "Pre-release dry run"
# 5. Delete the tag
git tag -d v1.0.0-test
```

**This run is called a "dry run".**
**A release without a dry run is wishful thinking.**

---

## 7. Fourth Act · Action · T-0

### 7.1 Formal Preparation

On the morning of T-0, **before beginning**, do four things:

```text
[ ] Close all IDEs
[ ] Close Slack / Teams / DingTalk
[ ] Close all StackOverflow tabs in the browser
[ ] Brew a cup of coffee (optional)
```

### 7.2 Three Sticks of Incense

See [`Incense.md`](./Incense.md) for the full process.

Brief version:

```text
First stick  · honoring Code    →  dotnet build -c Release → 0 error 0 warning
Second stick · honoring Tests   →  dotnet test  -c Release → all pass
Third stick  · honoring Future  →  git status → clean, CHANGELOG updated, version bumped
```

Only proceed after all three sticks have burned out.

### 7.3 Place the Tag

```powershell
# Confirm current branch
git branch --show-current  # Should output main or release/vX.Y.Z

# Confirm the latest commit is the one to release
git log --oneline -5

# Place an annotated tag (annotated is mandatory)
git tag -a v1.0.0 -m "Release v1.0.0 - Full Release Ritual"

# Check the tag content
git show v1.0.0
```

**A mistyped tag is the most unforgivable error in the release ritual.**
See [`Taboos.md`](./Taboos.md).

### 7.4 Release

```powershell
dotnet publish -c Release -p:Platform=x64 `
  -p:RuntimeIdentifier=win-x64 `
  -p:WindowsPackageType=MSIX `
  -p:WindowsAppSDKSelfContained=false `
  -p:GenerateAppxPackageOnBuild=true `
  -p:AppxPackageSigningEnabled=false
```

**While the command is running:**

- **Do not switch windows.**
- **Do not reply to messages.**
- **Do not rehearse in your head what you are going to say next.**
- **Just watch the output.**

**Watching the output is a form of practice.**

### 7.5 Push

```powershell
# Push the code
git push origin main

# Push the tag
git push origin v1.0.0
```

**Pushing the tag is the last step.**
**After the tag is pushed, change nothing.**

### 7.6 Announce

```markdown
# Release v1.0.0

We are pleased to announce FluentGit v1.0.0.

Release notes: [CHANGELOG.md](link)
Download: [GitHub Releases](link)

Special thanks to all contributors.

> "FluentGit, from today, can be used from scratch by someone who has never used it."
```

**Do not write in the announcement:**
- "This is the last change"
- "Should be fine"
- "Works on my machine"

**Must write in the announcement:**
- Version number
- Release date
- Known issues (if any)
- Download link

---

## 8. Fifth Act · Verification · T+1

### 8.1 Verify the Release

Within 24 hours after release, perform the following verification:

```text
[ ] Download the MSIX package from the official channel
[ ] Install on a clean Windows 10 22H2 machine
[ ] Install on a clean Windows 11 machine
[ ] Launch the app and confirm the window displays correctly
[ ] Initialize a test repository
[ ] Perform one pull / commit / push
[ ] Confirm InfoBar notifications work
[ ] Check Mica backdrop (Win11)
[ ] Check fallback backdrop (Win10)
```

**If any item fails, activate the rollback plan immediately.**

### 8.2 Monitor for Anomalies

```text
[ ] Check the Issue list for new bug reports
[ ] Check CI status for abnormal failures
[ ] Check the logging system for new Error-level logs
[ ] Check user groups / forums for negative feedback
```

**The 24 hours after release is the most fragile 24 hours.**
**Do not make any other significant change during these 24 hours.**

### 8.3 Rollback Decision

If a severe problem appears at T+1, there are two choices:

**Option A · Hotfix**
- Branch from the current version
- Apply a minimal fix
- Release v1.0.1

**Option B · Rollback**
- Retract the current version
- Restore to the previous stable version
- Retrospect, and prepare again

**Decision criteria:**

| Issue Level | Handling |
|---|---|
| Cannot launch | Rollback immediately |
| Data corruption | Rollback immediately |
| Major feature unusable | Hotfix (within 24 hours) |
| General bug | Record, fix in v1.0.1 |
| UI polish | Record, next version |

**Rollback is not failure.**
**Rollback is responsibility to users.**

---

## 9. Sixth Act · Reflection · T+7

### 9.1 Retrospective

On T+7, **the team sits together** and asks four questions:

1. **What went well this release?**
2. **What could have been better this release?**
3. **Did we violate any taboo?** (See [`Taboos.md`](./Taboos.md))
4. **What do we change for the next release?**

**The conclusion of the retrospective becomes input for the next release.**

### 9.2 Archive

```powershell
# Archive materials related to this release
mkdir docs/releases/v1.0.0
cp CHANGELOG.md docs/releases/v1.0.0/
cp RELEASE_NOTES.md docs/releases/v1.0.0/
cp ROLLBACK_PLAN.md docs/releases/v1.0.0/
# Commit
git add docs/releases/v1.0.0
git commit -m "docs: archive v1.0.0 release materials"
```

**Archiving is not about leaving a trace.**
**Archiving is so that three months from now, you do not have to guess.**

### 9.3 Dedication

On the evening of T+7, **relax**.

Do not look at code.
Do not look at issues.
Do not look at Slack.

**You just finished a release.**
**That deserves a good meal.**

---

## 10. The Seven Prohibitions

### First Prohibition · Do Not Skip T-7

Do not begin feature freeze only at T-0.

**Feature freeze must begin seven days early.**
**Seven days is the time that lets everything unfinished be quietly set aside.**

### Second Prohibition · Do Not Add Features After T-3

After T-3, only bug fixes are allowed.

**Any "while we're at it, let's add a small feature" thought belongs to v1.0.1.**

### Third Prohibition · Do Not Modify Anything Other Than CHANGELOG at T-1

T-1 only does three things:
1. Read CHANGELOG
2. Read the version declaration
3. Rehearse the three sticks

**Do not change code on this day.**

### Fourth Prohibition · Do Not Skip the Dry Run

The T-1 evening dry run cannot be skipped.

**The value of the dry run is that it exposes ahead of time what you have not prepared.**

### Fifth Prohibition · Do Not Change Code at T-0

On T-0, **write no new code**.

If a bug is found, choose:
- **Small bug**: Record it, fix in v1.0.1
- **Large bug**: Cancel the release, return to T-1

**Changing code at T-0 is the same as destroying the entire ritual.**

### Sixth Prohibition · Do Not Make New Changes at T+1

T+1 is **verification day**, not **development day**.

**Unless a hotfix is needed, change nothing at T+1.**

### Seventh Prohibition · Do Not Skip the T+7 Retrospective

Do not skip the retrospective just because "this one went smoothly".

**A smooth release is precisely the one most worth retrospecting.**
**Because it tells you: what you did right.**

---

## 11. Merit & Dedication

| Action | Merit |
|---|---:|
| Complete all six acts | +66 |
| Feature freeze on time at T-7 | +10 |
| Release notes and rollback plan ready at T-3 | +15 |
| Dry-run three sticks at T-1 | +10 |
| Release succeeds first try at T-0 | +50 |
| Verification completed within 24 hours at T+1 | +20 |
| Serious retrospective at T+7 | +30 |
| Skipping T-7 | −30 |
| Changing code at T-0 | −100 |
| Severe issue at T+1 but rolled back in time | +5 (attitude acceptable) |
| Releasing without a rollback plan | −200 |
| Skipping the T+7 retrospective | −50 |

### Dedication

> May this ritual
> purify seven days of mind,
> prepare three days of care,
> recite one day of words,
> act in one moment of stillness,
> verify one day of steadiness,
> reflect seven days of clarity.
>
> May this release
> succeed on the first try, and no one lose sleep.
> If not,
> may the rollback be smooth and the retrospective fruitful.

---

## 12. FAQ

**Q1: Six acts — is that too heavy?**
A: No.
**Six acts are six interfaces.**
**Interfaces are not burdens. They are boundaries.**

One week of ritual in exchange for a quiet release.
**Worth it.**

**Q2: If the project is small, must I still run the full process?**
A: No.
**Small projects can compress time, but cannot compress acts.**

```text
Large project: T-7  →  T-3  →  T-1  →  T-0  →  T+1  →  T+7
Small project: T-2h →  T-1h →  T-30m →  T-0  →  T+30m →  T+1d
```

**Time can be compressed. Acts cannot be skipped.**

**Q3: What if my boss asks me to skip T-7?**
A: You may, but ask the boss to **confirm in writing**.
**What is confirmed in writing is what counts as responsibility.**

**Q4: What if the T-1 dry run fails?**
A: Postpone the release.
**A failed dry run means something is not ready.**
**Postponing by one day is much cheaper than an incident after release.**

**Q5: If a severe issue appears at T+1, can I change code immediately?**
A: Yes, but follow the hotfix process:
1. Branch from the current tag
2. Make only the minimal change
3. Release v1.0.1

**Do not change code directly on the main branch.**
**Every direct commit on main is a seed for the next release incident.**

**Q6: Who attends the T+7 retrospective?**
A: **Everyone who participated in this release.**
Including:
- Those who wrote the code
- Those who tested the code
- Those who packaged
- Those who wrote the documentation
- Those who decided

**No bystanders. Everyone is a participant.**

**Q7: During the retrospective, can I criticize people?**
A: You may not criticize people.
**You may criticize processes, decisions, tools.**
**You may not criticize individuals.**

**The point of the retrospective is to improve the system, not to assign blame.**

**Q8: Why is the final act called "Reflection"?**
A: Because release is not the end.
**A release is the starting point of a loop.**

After T+7, you return to T-7 for the next one.
**Release is an eternal cycle.**

---

## Closing

The release ritual is not dogma.

It is **a map that a group of people drew together, after incident upon incident**.

This map is not necessarily perfect.
But at least it tells you: **where the cliffs are.**

**You may choose not to walk this map.**
**But when you walk, please remember to look back.**

Look at those who fell off the cliffs before,
**and what they paid to buy this map.**

---

> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
>
> One purification, one preparation, one recitation,
> one action, one verification, one reflection.
>
> May this release
> complete all six acts,
> stir up no trouble,
> and leave no one sleepless.

---

*Ritual location: every day from T-7 to T+7*
*Frequency: every formal release*
*Guardian status: permanent*
*Merit: see Chapter 11*
