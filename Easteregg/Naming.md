# ✍️ 代码玄学 · 命名篇 · Code Metaphysics · Naming

> “名字起得太嚣张，Bug 就找上门。”
> “Name it too arrogantly, and the bugs will find you.”

英文版：[Naming.en.md](./Naming.en.md)

---

## 一、缘起

FluentGit 曾经有过一个方法，叫 ExecutePerfectly。

写它的人是我。当时我很有信心。
一周之后，这个方法出了三个 Bug。
两周之后，我把它改名成 TryExecute。
三周之后，我把它拆成五个方法，改名成 ExecuteWithFallback。

从那以后，我明白了一个道理：
命名不是修辞。命名是预言。

你给一个方法起名叫 Perfect，宇宙就会派一个 Bug 来证明你不 Perfect。
你给一个变量起名叫 temp，它就会永远留在代码里，变成 temp2、temp3、tempFinal。

命名不是给代码起的。
命名是给未来的人起的。
而独自开发时，那个“未来的人”，首先就是三个月后的我自己。

---

## 二、命名的三重玄学

### 2.1 第一重 · 名实相符（Name Reality）

**代码是现实的投影。名字是投影的标签。**

如果一个变量叫 `userCount`，它就应该是**用户的个数**。
不是“近似用户数”，不是“缓存用户数”，不是“用户数 + 3”。

如果你不能为这个名字写出一句准确的注释，**这个名字就是错的**。

```csharp
// ❌ 名字撒谎
int userCount = users.Where(u => u.IsActive).Count() + pendingUsers.Count;

// ✅ 名实相符
int activeUserCount = users.Count(u => u.IsActive);
int pendingUserCount = pendingUsers.Count;
```

**名不副实的变量，是未来 Bug 的种子。**

---

### 2.2 第二重 · 名不狂妄（Name Modesty）

**给变量起的名字，不能超过它的实力。**

| 命名 | 隐含宣言 | 现实 |
|---|---|---|
| `finalResult` | “这就是最终结果” | 三天后被重写 |
| `perfectMatch` | “完美匹配” | 漏了一个边界条件 |
| `alwaysTrue` | “永远为真” | 网络断了就 false |
| `universalParser` | “万能解析器” | 只能解析 JSON |
| `masterService` | “主服务” | 和另外三个 `master` 打架 |

**宇宙会对每一句狂妄的话，收税。**
**税的形式，是一个新 Bug。**

**命名谦逊守则：**

```csharp
// ❌ 狂妄
public Result ExecutePerfectly() { }
public bool AlwaysValid { get; }
public class UniversalRepository { }

// ✅ 谦逊
public Result TryExecute() { }
public bool IsValidAtTimeOfCheck { get; }
public class Repository { }
```

**加一个 `Try`，加一个 `AtTimeOfCheck`，宇宙就不会找你麻烦。**

---

### 2.3 第三重 · 名不留恋（Name Non-Attachment）

**名字是暂定的。**

当代码变了，名字要跟着变。
不舍得改名字的人，最终会写出一个名字和内容完全无关的类。

```csharp
// 时间线：
// Day 1:  public class UserService
// Day 30: public class UserService // 里面也管 Order
// Day 60: public class UserService // 里面也管 Order、Payment、Log
// Day 90: public class UserService // 里面什么都有。名字已经没意义了。
```

**改名字的成本，是不改名字的百分之一。**

IDE 的 `Ctrl + R, Ctrl + R` 只需要三秒。
你的犹豫，需要三个月。

**不要留恋名字。**
**名字只是代码现在的样子。**

---

## 三、命名的具体戒律

以下八条，是项目组的共识。每一条背后，都是一个曾让人头疼的变量。

### 第一律 · 不可用 `temp` / `tmp` / `t`

```csharp
// ❌ 禁止
var temp = GetUser();
var tmp = temp.Name;
var t = tmp.Trim();
```

`temp` 是**命名的投降**。
你不知道它是什么，所以你叫它 `temp`。
**但你知道它是什么。你只是懒得想。**

```csharp
// ✅ 合格
var currentUser = GetUser();
var displayName = currentUser.Name;
var trimmedName = displayName.Trim();
```

**若你在一个函数里写超过三个 `temp`，请重新看一遍这个函数的职责。**

---

### 第二律 · 不可用单字母（`i` / `j` / `k` 除外）

```csharp
// ❌ 禁止
var x = CalculateTotal(items);
var y = x * 0.13m;
var z = y + shipping;

// ✅ 合格
var subtotal = CalculateTotal(items);
var tax = subtotal * TaxRate;
var grandTotal = tax + shipping;
```

**单字母变量是数学家的特权。**
**你不是数学家。你是在写一个半年后还要维护的软件。**

例外：`i` / `j` / `k` 用于简单的循环，`e` 用于 `catch (Exception e)`。
**只有这三个。第四个开始，你就是在偷懒。**

---

### 第三律 · 不可用缩写（除非是全行业公认）

```csharp
// ❌ 禁止
var usr = GetUsr();
var cfg = LoadCfg();
var repo = new Rpo();
var mgr = new Mgr();

// ✅ 合格
var user = GetUser();
var config = LoadConfiguration();
var repository = new Repository();
var manager = new Manager();
```

**唯一的例外：**

| 缩写 | 是否允许 | 原因 |
|---|:---:|---|
| `id` | ✅ | 全行业通用 |
| `url` | ✅ | 全行业通用 |
| `html` | ✅ | 全行业通用 |
| `json` | ✅ | 全行业通用 |
| `api` | ✅ | 全行业通用 |
| `usr` | ❌ | 只省了 2 个字母 |
| `cfg` | ❌ | 只省了 4 个字母 |
| `mgr` | ❌ | 只省了 4 个字母 |

**省 2 个字母，换来三个月的困惑。不划算。**

---

### 第四律 · 不可用拼音

```csharp
// ❌ 禁止
var yonghu = GetUser();
var peizhi = LoadConfig();
public class YongHuFuWu { }

// ✅ 合格
var user = GetUser();
var config = LoadConfig();
public class UserService { }
```

**拼音给懂中文的人看，勉强能懂。**
**给不懂中文的人看，就是乱码。**

你的代码库，可能明天就会有一个不懂中文的贡献者。

**用英文。哪怕你的英文不完美。**
**不完美的英文，也比完美的拼音好。**

---

### 第五律 · 不可用 `data` / `info` / `obj` / `item`

```csharp
// ❌ 禁止
var data = GetData();
var info = GetInfo();
var obj = GetObject();
var item = list[0];

// ✅ 合格
var repositories = GetRepositories();
var buildInfo = GetBuildInfo();
var project = GetProject();
var firstRepository = repositories[0];
```

`data`、`info`、`obj`、`item`，这四个词**没有信息**。

它们只是说“这是一个东西”。
但代码里的一切都是“一个东西”。

**如果一个变量只配叫 `data`，那说明你还没想清楚它是什么。**

---

### 第六律 · 不可反向命名（`notXxx`、`unXxx`）

```csharp
// ❌ 禁止
bool isNotReady = false;
bool hasNoPermission = true;
bool unregistered = false;

// 阅读时：
// isNotReady == false → 是 ready 吗？还是不是 not ready？哦是 ready。
```

**双重否定是 Bug 的温床。**

```csharp
// ✅ 合格
bool isReady = false;
bool hasPermission = true;
bool isRegistered = false;
```

**让名字描述“是什么”，而不是“不是什么”。**

---

### 第七律 · 不可在名字里带数字（`v2` / `new` / `old` 除外）

```csharp
// ❌ 禁止
var user1 = GetUser();
var user2 = GetOtherUser();
var result2 = Process(result1);

// ✅ 合格
var currentUser = GetUser();
var previousUser = GetOtherUser();
var processedResult = Process(rawResult);
```

`xxx2` 出现的瞬间，说明**你已经不确定 xxx1 和 xxx2 的区别了。**
你只是想让编译器通过。

**`xxx2` 是命名失败的墓志铭。**

例外：
- `retryCount2`？→ 改为 `secondRetryCount` 或 `retryCountAfterBackoff`。
- `v2` 用于**版本号**，是允许的。

---

### 第八律 · 不可用“语义空洞”的词

| 词 | 问题 | 换成 |
|---|---|---|
| `handle` | 处理什么？ | `Process` / `Dispatch` / `Route` |
| `manage` | 管理什么？ | `Register` / `Configure` / `Schedule` |
| `util` | 什么工具？ | 按具体职责命名 |
| `helper` | 帮什么忙？ | 按具体职责命名 |
| `common` | 什么共同？ | 按具体职责命名 |
| `core` | 什么是核心？ | 按具体职责命名 |
| `base` | 什么是基础？ | 按具体职责命名 |

**`Utils.cs`、`Helpers.cs`、`Common.cs`、`Core.cs`**
**这四个文件名，是代码库腐烂的四个征兆。**

```
如果你在写一个 Utils.cs，停下来。
问自己："这里的东西，能不能放到它们该在的地方？"
```

---

## 四、命名与心理

命名不只是技术问题。它是**心理问题**。

| 你起的名字 | 你的心态 |
|---|---|
| `temp` | 我不知道它是什么，也不想现在知道 |
| `data` | 我不想花时间想名字 |
| `userManagerHelper` | 我不确定职责，先堆着 |
| `doEverything` | 我已经放弃了 |
| `handleSpecialCase` | 这是我自己都没搞懂的一坨 |
| `fixForBug1234` | 我正在打补丁，而不是在解决问题 |

**看懂一个人写的名字，就能看懂他当时的状态。**

**代码会骗人。命名不会。**

---

## 五、命名的正面清单

好名字长什么样？看这些：

```csharp
// 变量
var activeRepositories = GetActiveRepositories();
var lastCommitAt = repository.LastCommitAt;
var isDirty = workingTree.HasChanges;

// 方法
public Task<IReadOnlyList<Repository>> LoadRepositoriesAsync()
public bool TryParseRemoteUrl(string input, out Uri? url)
public void ApplyTheme(ElementTheme theme)

// 类
public sealed class RepositoryLoader
public record CommitSummary(string Sha, string Message, DateTimeOffset AuthorAt)
public interface IRepositoryStore

// 布尔
public bool IsEmpty => Items.Count == 0;
public bool HasUncommittedChanges { get; }
public bool CanPush { get; }
```

**规律：**
- 名词是**具体**的（`RepositoryLoader`，不是 `Manager`）。
- 动词是**动作**（`Load`、`Apply`、`TryParse`）。
- 布尔以 `Is` / `Has` / `Can` 开头。
- 复数用 `s`，不规则复数按英文习惯。

---

## 六、功德与回向

| 行为 | 功德 |
|---|---:|
| 改掉一个 `temp` 命名 | +5 |
| 改掉一个 `data` 变量 | +5 |
| 把一个 `Utils.cs` 拆成三个职责明确的文件 | +30 |
| 给一个方法改一个更准确的名字 | +10 |
| 起了一个完美的名字，全组称赞 | +20 |
| 写了一个新 `temp` | −5 |
| 写了一个新的 `userManagerHelper` | −20 |
| 写了一个新 `Utils.cs` | −50 |
| 写了 `ExecutePerfectly` | −100（会被 Reviewer 笑很久） |

### 回向

> 愿此命名，
> 一名字，说清是什么。
> 一名字，说清做什么。
> 一名字，说清为什么。
>
> 愿你的代码，
> 三年后打开，还能读懂。

---

## 七、常见问题

**Q1：名字太长怎么办？**
A：长不是问题。**看不懂才是问题。**

```csharp
// 长，但清楚
var repositoriesWithUncommittedChanges = GetRepositoriesWithUncommittedChanges();

// 短，但看不懂
var rwuc = GetRWUC();
```

**代码的阅读时间是写作时间的十倍。长一点，值得。**

**Q2：我英文不好，能用中式英文命名吗？**
A：能用**不完美**的英文，不能用**中式英文**。

```csharp
// 不完美，但能懂 ✅
var userInformation = GetUserInformation();

// 中式英文 ❌
var userMessageInfo = GetUserMessageInfo();  // 什么意思？
var openFlag = true;                          // 开什么？
```

**宁可用一个笨拙但完整的英文，也不要用一个看起来很顺口但语义模糊的“中式英文”。**

**Q3：公司有命名规范吗？**
A：有的话，遵循规范。
没有的话，**这份文档就是**。

**Q4：为什么 `Manager` 不算好名字？**
A：因为 `Manager` 不说明**管理什么**、**怎么管**。

```csharp
// ❌
class UserManager { }      // 管理用户的什么？权限？数据？会话？

// ✅
class UserSessionManager { }   // 管理用户会话
class UserPermissionStore { }  // 存储用户权限
class UserAuthenticator { }    // 认证用户
```

**`Manager` 是设计不清晰的信号。**

**Q5：那 `Service` 呢？**
A：`Service` 也是弱命名，但**在架构层面可以接受**。

`UserService` 至少让人知道“这是用户相关的服务”。
比 `UserManager` 好。但不如 `UserAuthenticator`。

**`Service` 是弱命名中的最强。**
**它是及格线。但不要停在及格线上。**

**Q6：命名有没有“绝对正确”的标准？**
A：没有。
但有“绝对错误”的——**说不出它是什么的，就是错的**。

**Q7：如果我发现了一个历史遗留的烂名字，要改吗？**
A：要。但分批改。

- 不要在一个 PR 里改 500 个名字。Reviewer 会崩溃。
- 每次改一个文件，或者一个模块。
- 在 PR 描述里说清：“本次仅重命名，无逻辑改动。”

**改名字是功德。**
**一次性改一万个名字，是灾难。**

**Q8：`Foo` / `Bar` / `Baz` 能用吗？**
A：**只能用在测试和示例里。**
不能出现在产品代码中。

**Q9：`_value` 这种下划线开头呢？**
A：这是私有字段的常见约定，允许。但要看项目规范。

**C# 中更常见的约定是：**
```csharp
private readonly string _name;    // 私有字段，下划线前缀
public string Name => _name;      // 公开属性
```

**跟随项目。没有项目就跟随这份文档。**

**Q10：为什么这份文档的标题这么长？**
A：`代码玄学 · 命名篇` 本身就是一份命名。
**玄学是幌子，命名是正经事。**

---

## 结语

命名是一件小事。

小到很多人觉得“起个名字而已，随便写写”。
**也小到，没有人会专门为它写一份文档。**

但它**每天都要被阅读一百次**。

你起的名字，会在三年后的某个深夜，被另一个工程师读到。
那时候你已经不在这个项目。
那时候你不知道他在骂你，还是在感谢你。

**你今天的名字，就是他今晚的心情。**

所以：

- 认真起名。
- 名字要谦虚。
- 名字要说真话。

**名字，是对未来的自己，最大的善意。**

---

> 南无本师释迦牟尼佛
> 南无本师释迦牟尼佛
> 南无本师释迦牟尼佛
>
> 愿此代码，
> 三年后仍能读懂。
> 愿此命名，
> 三年后仍不脸红。

---

*命名规则位置：本项目所有 `.cs` 文件的标识符*
*生效时间：从你写第一行代码开始*
*守护状态：常驻*
*功德：见第六章*