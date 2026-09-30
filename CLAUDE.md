# Word Battle — Claude Guide

A Unity/C# spiritual successor to **Quarrel** (Denki, "Scrabble × Risk × Countdown"). Players capture territory by winning **word battles**: both sides get the same 8 letter tiles and a shared 30s timer, and the higher-scoring valid word wins.

We're building the **MVP**: a 1v1, best-of-3 word battle with no territory yet. The architecture must still grow into the full Risk-style game without a rewrite.

## Read before working

| Doc | What's in it |
|---|---|
| `Docs/game-design.md` | Rules, letter scores, MVP spec, full-game vision, **open design questions** |
| `Docs/architecture.md` | Layers, managers, state machines, current code vs target, known issues |
| `Docs/roadmap.md` | Phase status and feature backlog (what's done, what's next) |
| `Docs/decisions.md` | Decisions already made (don't re-litigate them); add new ones here |
| `System Diagrams/` | The user's original ASCII state diagrams (GameMode, TurnState, Battle) |

## Who I'm working with

The user is an experienced programmer who is **new to game development and Unity**. Skip explanations of general programming. Do explain Unity-specific concepts when they come up for the first time: MonoBehaviour lifecycle, serialization/Inspector, prefabs, scenes, `.meta` files, and the main thread. When a change needs manual Editor work (assigning Inspector references, creating GameObjects), give exact step-by-step instructions, or do it through the Unity MCP when possible.

## Handling a feature request

1. Find the feature in `Docs/roadmap.md` and check `Docs/game-design.md` → *Open questions*. If the feature depends on an unresolved question, **ask before building**. Use the recommended default listed there as the suggested option.
2. For anything touching more than one manager, sketch the design against `Docs/architecture.md` first (the `game-designer` / `gameplay-engineer` agents can help).
3. Implement: pure logic first (testable), then the MonoBehaviour adapter, then scene/UI wiring.
4. Verify: check the Unity console via MCP (`Unity_GetConsoleLogs`), then run tests when they exist.
5. Update the docs you invalidated: roadmap status, architecture "current state", decisions log.

## Project agents (`.claude/agents/`)

| Agent | Use for |
|---|---|
| `game-designer` | Turning a feature idea into a spec: rules, edge cases, balance, open questions. Read-only. |
| `gameplay-engineer` | C# gameplay systems: state machines, managers, battle/turn logic, AI opponent. |
| `word-engine` | Dictionary pipeline, rack/tile generation, word validation against a rack, anagram/solver, AI word choice. |
| `unity-ui-engineer` | UGUI/TextMeshPro screens, scene and prefab wiring, Inspector setup, and the Unity MCP. |
| `test-engineer` | Unity Test Framework (EditMode/PlayMode), asmdefs, and test coverage of rules. |
| `unity-reviewer` | Reviewing diffs for Unity pitfalls, architecture drift, and correctness. Read-only. |

## Tech facts

- **Unity 6000.3.7f1** (Unity 6.3), Universal 2D (URP) template.
- **UI:** UGUI + TextMeshPro. **Input:** the new Input System package (`com.unity.inputsystem`) is installed.
- **Test Framework:** 1.6.0 is installed; no tests or asmdefs exist yet.
- **Scene:** `Assets/Scenes/WordBattle.unity`.
- **Scripts:** `Assets/Scripts/{Core,Gameplay,Utils}`.
- **Dictionary:** `Assets/Data/WordBattleDictionary.txt` — lowercase, one word per line, LF line endings, 172,823 words. The source is **ENABLE1** (public domain).
- **Version control:** Git + GitHub (`Tedgar20/...`). Feature branches are named `feature/<Name>`, with PRs into `main`. Commit `.meta` files together with their assets. Never commit `Library/`, `Temp/`, `Logs/`, or `UserSettings/`.
- **Unity MCP (`unity-mcp`):**
  - `Unity_GetConsoleLogs` reads compile errors and logs. Use it after every script change.
  - `Unity_RunCommand` runs editor C#.
  - `Unity_SceneView_*` / `Unity_Camera_Capture` take screenshots.
  - The `NoSubscription` info logs come from the Unity AI package and are harmless.

## Code conventions

- **File names:** one public type per file, and the file name must equal the class name. Unity can't attach a MonoBehaviour otherwise.
- **Logic vs Unity:** game rules live in **plain C# classes** (no `UnityEngine` dependency where practical) so EditMode tests can cover them. MonoBehaviours are thin adapters that handle lifecycle, Inspector refs, and UI.
- **Serialized fields:** use `[SerializeField] private` rather than public fields.
- **Validation:** validate required Inspector references in `Awake` and log a clear error.
- **Events:** managers communicate through C# events/`Action`s or direct calls from the layer above. Lower layers never reach up (Battle doesn't know about Turns; Turns don't know about screens).
- **Words:** uppercase words internally. Letters are A–Z only.
- **Style:** match the existing code (Allman braces, `PascalCase` methods, `camelCase` fields). Namespaces/asmdefs are proposed in `Docs/decisions.md`, not adopted yet.
