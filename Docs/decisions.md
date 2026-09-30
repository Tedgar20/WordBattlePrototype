# Decision Log

Newest last. Don't re-open an **Accepted** decision without the user's say-so. Add new decisions as they're made.

| # | Decision | Status | Why |
|---|---|---|---|
| D1 | Unity (C#) over Unreal | Accepted | C# fits the user's background; well suited to 2D and UI-heavy games; broad platform support |
| D2 | Git + GitHub, not Unity Version Control | Accepted | User preference; PR workflow |
| D3 | Universal 2D (URP) template | Accepted | 2D, UI-heavy game |
| D4 | MVP is 1v1, best-of-3, with no territory | Accepted | Prove the core battle loop first |
| D5 | The opponent is a simple AI, not a local human | Accepted | Simultaneous hidden input doesn't work with hotseat play |
| D6 | Game mode, turns, and battles are separate layers; battles are isolated and disposable | Accepted | Territory and turns must slot in without rewriting battles |
| D7 | Turn timer and battle timer are separate; the turn timer pauses during battles | Accepted | Matches the full-game design |
| D8 | Battles always produce a winner; ties are broken internally | Accepted | From the MVP bible; the rule is D14 |
| D9 | Best-of-N is configurable (MVP 3, production 1) | Accepted | Avoid a rewrite later |
| D10 | Losing a battle doesn't end the turn; attacking needs ≥ 2 troops | Accepted (full game) | From the MVP bible |
| D11 | Game rules live in plain C#, with MonoBehaviours as thin adapters | Proposed | Testability, determinism, and a networking seam |
| D12 | Add a `WordBattle` namespace and asmdefs (`WordBattle.Core`, `WordBattle.Game`, `WordBattle.Tests.EditMode`) | Proposed | Required for EditMode tests; faster compiles |
| D13 | Must be legally distinct from Quarrel (name, characters, art) | Accepted | Legal |
| D14 | Tie-break: the fastest submission wins; if neither player submitted, the defender wins | Accepted | User decision; the attacker chose to fight. Record the submit time for each player; battles need attacker/defender roles |
| D15 | No score bonus for using all 8 tiles; gold is visual only | Accepted | User decision |
| D16 | Word list is ENABLE1 (public domain); the user supplies the definitions dictionary, stored in the repo | Accepted | User decision; licensing is clear |
