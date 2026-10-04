
# ✍️ Code Metaphysics · Naming

> "Name it too arrogantly, and the bugs will find you."

Chinese version: [Naming.md](./Naming.md)

---

## 1. Origin

FluentGit once had a method called ExecutePerfectly.

I wrote it. I was confident.
A week later, that method had three bugs.
Two weeks later, I renamed it TryExecute.
Three weeks later, I split it into five methods and renamed it ExecuteWithFallback.

Since then, I have understood one thing:
Naming is not rhetoric. Naming is prophecy.

Name a method Perfect, and the universe will send a bug to prove you are not Perfect.
Name a variable temp, and it will stay in the codebase forever, becoming temp2, temp3, tempFinal.

Naming is not for the code.
Naming is for whoever reads it in the future.
When you develop alone, that future reader is first of all you, three months later.
---

## 2. The Threefold Metaphysics of Naming

### 2.1 First Layer · Name Reality

**Code is a projection of reality. A name is the label on that projection.**

If a variable is called `userCount`, it should be **the number of users**.
Not "approximate user count", not "cached user count", not "user count + 3".

If you cannot write one accurate sentence of documentation for that name, **the name is wrong**.

```csharp
// ❌ Name is lying
int userCount = users.Where(u => u.IsActive).Count() + pendingUsers.Count;

// ✅ Name matches reality
int activeUserCount = users.Count(u => u.IsActive);
int pendingUserCount = pendingUsers.Count;
```

**A name that does not match its value is a seed for a future bug.**

---

### 2.2 Second Layer · Name Modesty

**A variable's name must not exceed its actual power.**

| Name | Implied Claim | Reality |
|---|---|---|
| `finalResult` | "This is the final result" | Rewritten three days later |
| `perfectMatch` | "Perfect match" | Missed one edge case |
| `alwaysTrue` | "Always true" | false when the network drops |
| `universalParser` | "Parses anything" | Only parses JSON |
| `masterService` | "The master service" | Fights with three other `master`s |

**The universe taxes every arrogant claim.**
**The tax is a new bug.**

**Naming modesty guidelines:**

```csharp
// ❌ Arrogant
public Result ExecutePerfectly() { }
public bool AlwaysValid { get; }
public class UniversalRepository { }

// ✅ Modest
public Result TryExecute() { }
public bool IsValidAtTimeOfCheck { get; }
public class Repository { }
```

**Add a `Try`. Add an `AtTimeOfCheck`. The universe leaves you alone.**

---

### 2.3 Third Layer · Name Non-Attachment

**Names are temporary.**

When the code changes, the name must change with it.
Those who refuse to rename eventually produce a class whose name has nothing to do with its contents.

```csharp
// Timeline:
// Day 1:  public class UserService
// Day 30: public class UserService // also handles Orders
// Day 60: public class UserService // also handles Orders, Payments, Logs
// Day 90: public class UserService // handles everything. The name means nothing.
```

**The cost of renaming is one percent of the cost of not renaming.**

The IDE's `Ctrl + R, Ctrl + R` takes three seconds.
Your hesitation takes three months.

**Do not become attached to names.**
**A name is only what the code looks like right now.**

---

## 3. Concrete Rules of Naming

The following eight rules are the team's consensus. Behind each one is a variable that once caused pain.

### Rule One · Do Not Use `temp` / `tmp` / `t`

```csharp
// ❌ Forbidden
var temp = GetUser();
var tmp = temp.Name;
var t = tmp.Trim();
```

`temp` is **naming surrender**.
You don't know what it is, so you call it `temp`.
**But you do know. You're just too lazy to think.**

```csharp
// ✅ Acceptable
var currentUser = GetUser();
var displayName = currentUser.Name;
var trimmedName = displayName.Trim();
```

**If you write more than three `temp`s in one function, reconsider that function's responsibility.**

---

### Rule Two · Do Not Use Single Letters (Except `i` / `j` / `k`)

```csharp
// ❌ Forbidden
var x = CalculateTotal(items);
var y = x * 0.13m;
var z = y + shipping;

// ✅ Acceptable
var subtotal = CalculateTotal(items);
var tax = subtotal * TaxRate;
var grandTotal = tax + shipping;
```

**Single-letter variables are a mathematician's privilege.**
**You are not a mathematician. You are writing software that will be maintained for the next six months.**

Exception: `i` / `j` / `k` for simple loops, `e` for `catch (Exception e)`.
**Only those three. The fourth is laziness.**

---

### Rule Three · Do Not Use Abbreviations (Unless Universally Recognized)

```csharp
// ❌ Forbidden
var usr = GetUsr();
var cfg = LoadCfg();
var repo = new Rpo();
var mgr = new Mgr();

// ✅ Acceptable
var user = GetUser();
var config = LoadConfiguration();
var repository = new Repository();
var manager = new Manager();
```

**The only exceptions:**

| Abbreviation | Allowed | Reason |
|---|:---:|---|
| `id` | ✅ | Universally recognized |
| `url` | ✅ | Universally recognized |
| `html` | ✅ | Universally recognized |
| `json` | ✅ | Universally recognized |
| `api` | ✅ | Universally recognized |
| `usr` | ❌ | Saves only 2 letters |
| `cfg` | ❌ | Saves only 4 letters |
| `mgr` | ❌ | Saves only 4 letters |

**Save 2 letters, buy three months of confusion. Not worth it.**

---

### Rule Four · Do Not Use Pinyin

```csharp
// ❌ Forbidden
var yonghu = GetUser();
var peizhi = LoadConfig();
public class YongHuFuWu { }

// ✅ Acceptable
var user = GetUser();
var config = LoadConfig();
public class UserService { }
```

**Pinyin is barely readable for Chinese speakers.**
**For anyone else, it is gibberish.**

Your codebase may have a non-Chinese contributor tomorrow.

**Use English. Even if your English is imperfect.**
**Imperfect English is better than perfect Pinyin.**

---

### Rule Five · Do Not Use `data` / `info` / `obj` / `item`

```csharp
// ❌ Forbidden
var data = GetData();
var info = GetInfo();
var obj = GetObject();
var item = list[0];

// ✅ Acceptable
var repositories = GetRepositories();
var buildInfo = GetBuildInfo();
var project = GetProject();
var firstRepository = repositories[0];
```

`data`, `info`, `obj`, `item` — these four words **carry no information**.

They only say "this is a thing".
But everything in code is "a thing".

**If a variable only deserves the name `data`, you haven't figured out what it is yet.**

---

### Rule Six · Do Not Use Negative Names (`notXxx`, `unXxx`)

```csharp
// ❌ Forbidden
bool isNotReady = false;
bool hasNoPermission = true;
bool unregistered = false;

// When reading:
// isNotReady == false → is that ready? or not-not-ready? oh, ready.
```

**Double negation is a breeding ground for bugs.**

```csharp
// ✅ Acceptable
bool isReady = false;
bool hasPermission = true;
bool isRegistered = false;
```

**Let names describe what something is, not what it is not.**

---

### Rule Seven · Do Not Put Numbers in Names (Except `v2` / `new` / `old`)

```csharp
// ❌ Forbidden
var user1 = GetUser();
var user2 = GetOtherUser();
var result2 = Process(result1);

// ✅ Acceptable
var currentUser = GetUser();
var previousUser = GetOtherUser();
var processedResult = Process(rawResult);
```

The moment `xxx2` appears, it means **you are no longer sure about the difference between xxx1 and xxx2.**
You just want the compiler to pass.

**`xxx2` is the epitaph of naming failure.**

Exceptions:
- `retryCount2`? → Rename to `secondRetryCount` or `retryCountAfterBackoff`.
- `v2` for **version numbers** is allowed.

---

### Rule Eight · Do Not Use "Semantically Empty" Words

| Word | Problem | Replace With |
|---|---|---|
| `handle` | Handle what? | `Process` / `Dispatch` / `Route` |
| `manage` | Manage what? | `Register` / `Configure` / `Schedule` |
| `util` | What utility? | Name by actual responsibility |
| `helper` | Help with what? | Name by actual responsibility |
| `common` | Common to what? | Name by actual responsibility |
| `core` | What is core? | Name by actual responsibility |
| `base` | What is base? | Name by actual responsibility |

**`Utils.cs`, `Helpers.cs`, `Common.cs`, `Core.cs`**
**These four file names are four symptoms of codebase rot.**

```
If you are writing a Utils.cs, stop.
Ask yourself: "Can whatever goes here live where it actually belongs?"
```

---

## 4. Naming and Psychology

Naming is not just a technical problem. It is a **psychological problem**.

| Name You Chose | Your State of Mind |
|---|---|
| `temp` | I don't know what it is, and I don't want to know right now |
| `data` | I don't want to spend time thinking of a name |
| `userManagerHelper` | I'm unsure of the responsibility, so I stack it up |
| `doEverything` | I have already given up |
| `handleSpecialCase` | This is a mess even I don't understand |
| `fixForBug1234` | I'm patching, not solving |

**Read a person's names, and you will read their state of mind at the time.**

**Code can lie. Naming does not.**

---

## 5. The Positive Naming List

What does a good name look like? Look at these:

```csharp
// Variables
var activeRepositories = GetActiveRepositories();
var lastCommitAt = repository.LastCommitAt;
var isDirty = workingTree.HasChanges;

// Methods
public Task<IReadOnlyList<Repository>> LoadRepositoriesAsync()
public bool TryParseRemoteUrl(string input, out Uri? url)
public void ApplyTheme(ElementTheme theme)

// Classes
public sealed class RepositoryLoader
public record CommitSummary(string Sha, string Message, DateTimeOffset AuthorAt)
public interface IRepositoryStore

// Booleans
public bool IsEmpty => Items.Count == 0;
public bool HasUncommittedChanges { get; }
public bool CanPush { get; }
```

**Patterns:**
- Nouns are **concrete** (`RepositoryLoader`, not `Manager`).
- Verbs are **actions** (`Load`, `Apply`, `TryParse`).
- Booleans start with `Is` / `Has` / `Can`.
- Plurals use `s`, irregular plurals follow English convention.

---

## 6. Merit & Dedication

| Action | Merit |
|---|---:|
| Renaming one `temp` | +5 |
| Renaming one `data` variable | +5 |
| Splitting a `Utils.cs` into three well-named files | +30 |
| Renaming a method to something more accurate | +10 |
| Naming something perfectly and the whole team praises it | +20 |
| Writing a new `temp` | −5 |
| Writing a new `userManagerHelper` | −20 |
| Writing a new `Utils.cs` | −50 |
| Writing `ExecutePerfectly` | −100 (the reviewer will laugh for a long time) |

### Dedication

> May this naming
> name what it is,
> name what it does,
> name why it exists.
>
> May your code,
> three years from now, still be readable.

---

## 7. FAQ

**Q1: What if the name is too long?**
A: Long is not a problem. **Unreadable is a problem.**

```csharp
// Long, but clear
var repositoriesWithUncommittedChanges = GetRepositoriesWithUncommittedChanges();

// Short, but unreadable
var rwuc = GetRWUC();
```

**Code is read ten times more than it is written. A little longer is worth it.**

**Q2: My English isn't great. Can I use "Chinglish" naming?**
A: Use **imperfect** English. Do not use **Chinglish**.

```csharp
// Imperfect but understandable ✅
var userInformation = GetUserInformation();

// Chinglish ❌
var userMessageInfo = GetUserMessageInfo();  // What does this mean?
var openFlag = true;                          // Open what?
```

**Prefer a clumsy but complete English name over a smooth-looking but semantically vague "Chinglish" name.**

**Q3: Does the company have naming standards?**
A: If yes, follow them.
If not, **this document is the standard.**

**Q4: Why isn't `Manager` a good name?**
A: Because `Manager` does not say **what** it manages or **how**.

```csharp
// ❌
class UserManager { }      // Manages what about users? Permission? Data? Session?

// ✅
class UserSessionManager { }   // Manages user sessions
class UserPermissionStore { }  // Stores user permissions
class UserAuthenticator { }    // Authenticates users
```

**`Manager` is a signal of unclear design.**

**Q5: What about `Service`?**
A: `Service` is also a weak name, but **it is acceptable at the architectural level**.

`UserService` at least tells you "this is a user-related service".
Better than `UserManager`. Worse than `UserAuthenticator`.

**`Service` is the strongest of the weak names.**
**It is the passing grade. Do not stop at the passing grade.**

**Q6: Is there an "absolutely correct" naming standard?**
A: No.
But there is "absolutely wrong" — **if you cannot say what it is, it is wrong**.

**Q7: If I find a legacy bad name, should I fix it?**
A: Yes. But in batches.

- Do not rename 500 things in one PR. The reviewer will collapse.
- Rename one file or one module at a time.
- In the PR description, state: "Pure rename, no logic change."

**Renaming is merit.**
**Renaming ten thousand things at once is a disaster.**

**Q8: Can I use `Foo` / `Bar` / `Baz`?**
A: **Only in tests and examples.**
Never in production code.

**Q9: What about underscore prefixes like `_value`?**
A: That is a common convention for private fields. Allowed. But follow project convention.

**In C#, a more common convention is:**
```csharp
private readonly string _name;    // private field, underscore prefix
public string Name => _name;      // public property
```

**Follow the project. If there is no project, follow this document.**

**Q10: Why is this document's title so long?**
A: `Code Metaphysics · Naming` is itself a name.
**The metaphysics is a pretext. Naming is the serious business.**

---

## Closing

Naming is a small thing.

So small that many think, "It's just a name, I'll write whatever".
**And so small that no one writes a document specifically for it.**

But it is **read a hundred times every day**.

The name you choose will be read at some late hour three years from now by another engineer.
By then you are no longer on this project.
By then you don't know whether they are cursing you or thanking you.

**The name you choose today is their mood tonight.**

So:

- Name carefully.
- Name modestly.
- Name truthfully.

**A name is the greatest kindness you can give to your future self.**

---

> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
> Namo Shakyamuni Buddha
>
> May this code
> still be readable three years from now.
> May this naming
> not make you blush three years from now.

---

*Location of naming rules: every identifier in every `.cs` file in this project*
*Effective since: the moment you wrote your first line of code*
*Guardian status: permanent*
*Merit: see Chapter 6*
