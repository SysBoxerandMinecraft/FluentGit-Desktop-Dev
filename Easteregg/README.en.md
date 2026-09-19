# 🥚 FluentGit Easteregg · Catalog of Easter Eggs and Cyber Superstitions

> "We are not superstitious. We merely observed that superstitious developers ship fewer bugs."

---

## 📖 Preface

FluentGit is a serious, modern Git GUI client built on WinUI 3 / Windows App SDK.
It is written in C# / .NET 10 and pursues the clarity and restraint of Fluent Design.

And this folder is the part of it that is not restrained.

Every document in this directory describes a piece of "cyber superstition" that truly exists (or is planned to exist) in the FluentGit codebase.
They do not affect any functionality, compilation, or runtime behavior—unless you delete them.
Delete them, and everything remains the same. But you will not sleep well at night.

> ⚠️ Disclaimer: All content in this directory is developer self-deprecating humor and engineering culture play. It does not represent any religious stance.
> We respect all religions and cultures. The Buddha here exists as a code guardian symbol.

---


## 🗂️ Easter Egg Index

Each Easter egg has a Chinese version and an English version. English versions end with `.en.md`.
Interactive Easter eggs (HTML) end with `.html` / `.en.html`.

| Easter Egg | Chinese Version | English Version | Effect | Merit |
|---|---|---|:---:|:---:|
| 🧘 Digital Buddha | [Buddha.md](./Buddha.md) | [Buddha.en.md](./Buddha.en.md) | Blesses code against bugs; protects `dotnet build` on the first try | +100 | 
| 🕯️ Incense Ritual | [Incense.md](./Incense.md) | [Incense.en.md](./Incense.en.md) | Must-do before release; three sticks for a successful first try | +10 / ritual |
| 🪵 Digital Wooden Fish | [WoodenFish.md](./WoodenFish.md) · [HTML](./WoodenFish.html) | [WoodenFish.en.md](./WoodenFish.en.md) · [HTML](./WoodenFish.en.html) | Tap once, Merit +1; eases code review anxiety | +1 / tap |
| 🚫 Developer Taboos | [Taboos.md](./Taboos.md) | [Taboos.en.md](./Taboos.en.md) | Things never to do, e.g. deploy on Friday | — |
| ✍️ Code Metaphysics · Naming | [Naming.md](./Naming.md) | [Naming.en.md](./Naming.en.md) | Never name a variable too arrogantly | — |
| 🔮 Bug Summoning / Banishing Circle | [BugSummon.md](./BugSummon.md) | [BugSummon.en.md](./BugSummon.en.md) | Reproduce a bug, or seal it in `#if false` | −50 | 
| 🦆 Rubber Duck Debugging | [RubberDuck.md](./RubberDuck.md) | [RubberDuck.en.md](./RubberDuck.en.md) | Explain it to the duck, problem disappears | +3 |
| 🎋 Fortune Draw | [Fortune.md](./Fortune.md) | [Fortune.en.md](./Fortune.en.md) | Decide whether to refactor today | ±? |
| 📜 Full Release Ritual | [Ritual.md](./Ritual.md) | [Ritual.en.md](./Ritual.en.md) | Complete prayer flow before v1.0.0 | +66 | 



---

## 🔢 Merit System

FluentGit internally maintains a purely fictional Merit system.

- Merit cannot be exchanged for any physical item, virtual item, license, or discount.
- Merit cannot be transferred, inherited, or reported lost.
- The only purpose of Merit is to make you hesitate for 0.5 seconds before a late-night `git push --force`.

| Action | Merit |
|---|---:|
| Digital Buddha permanently enshrined in `MainWindow.xaml.cs` | +100 |
| Burn incense before Release | +10 |
| Write a meaningful commit message | +5 |
| Add a unit test | +8 |
| Fix a bug you left three years ago | +20 |
| Write `// TODO: fix later` | −5 |
| Delete the `Easteregg/` folder | −∞ |

---

## 🚀 How to Use

1. Read: flip through it casually, for fun.
2. Perform: follow the rituals in the documents, such as burning incense before committing.
3. Contribute: want to add a new Easter egg? See the next section.
4. Spread: copy the ASCII Buddha from `Buddha.md` into your own project. The Buddha belongs to no single project, and Merit still counts.

---


---

## ⚖️ Disclaimer

1. Nothing in this directory constitutes any form of religious advice, financial advice, career advice, or life advice.
2. The Digital Buddha will not actually fix your bugs. What fixes your bugs is `git bisect`.
3. Not deploying on Friday does reduce incident rates. This one is true.
4. If you encounter build failures because you deleted files in this directory, that is your `csproj` problem, not a metaphysics problem.
   — But you should still add them back.

---

## 🧭 Closing

> Code is a product of logic, but the people who write code are not.
> What we need was never a Buddha, but a reason to tell ourselves at 3 a.m., "Just one more try."
>
> If that reason happens to be an ASCII Buddha, that's fine too.

Namo Shakyamuni Buddha.
May your `dotnet build` have zero warnings, and your `git merge` zero conflicts.

---

*Last updated: 2025 · FluentGit Team*