# Architecture

## Layers (strictly separated)

The game has three gameplay layers plus shared services. **Each layer only talks downward.** Upward communication happens through events/results.

```
GameModeManager      screens & lifecycle: MainMenu → GameSetup → Gameplay → Victory
   │ starts/ends
GameStateManager     overall match state; owns players; decides when the game is over
   │ drives
PlayerTurnManager    whose turn, turn timer (pauses during battle), attack / end-turn   ← full game
   │ requests battle
BattleManager        one battle: rack, 30s battle timer, submissions, locking, resolve, result
   │ uses
Services (plain C#)  DictionaryManager/WordValidator, LetterScoreTable, WordScorer, RackGenerator, AI opponent
```

For the **MVP**, `PlayerTurnManager` is effectively a pass-through: a match is 3 consecutive battles between the same two players. Keep the seam so territory turns can slot in later.

## State machines (from `System Diagrams/`)

```csharp
enum GameMode   { MainMenu, GameSetup, Gameplay, Victory }
enum TurnState  { StartTurn, AwaitAction, InBattle, EndingTurn }
// Battle: BattleStart → AcceptInputs (30s) → ResolveBattle → BattleResult → return to caller
```

- **Battle:** a battle is isolated and disposable. It receives a config (players, rack, time limit) and emits a `BattleResult` (words, scores, winner, tie-break reason). It knows nothing about turns or territory.
- **Best-of-N:** best-of-N is a *match/round* concern layered over single battles. Use `roundsToWin` (MVP: best-of-3 → 2 wins; production: 1). Don't bake the number into `BattleManager`.
- **Timers:**
  - The turn timer and the battle timer are separate objects.
  - Entering `InBattle` pauses the turn timer; the `BattleResult` resumes it.
  - During a battle, strategic UI is disabled and word input is enabled.

## Design principles

- **Pure C# core:**
  - Rules, scoring, validation, rack generation, battle resolution, and AI word choice are plain C# classes.
  - Time is passed in (`Tick(float dt)`), not read from `Time.deltaTime`, so everything is EditMode-testable and deterministic. That matters for future networking.
  - Randomness uses an injected seeded `System.Random`.
- **Thin MonoBehaviours:** they hold Inspector references, forward `Update` ticks, and bind events to UI.
- **Players as an abstraction:** human, AI, and later remote players implement a common interface (e.g. `IPlayerController` with a "submit word" callback). `BattleManager` treats them all the same. This is the multiplayer seam.
- **Dictionary data:** load once and share it. Use a `HashSet<string>` for validity; add a sorted-letters index when anagram lookup / AI is needed.

## Current state of the code (as of branch `feature/TurnSystem-GameState`)

The MVP loop is **playable end to end**: Main Menu → 3-round match against the AI → Victory → Play Again / Main Menu.

**Assemblies:**

| Assembly | Folder | Contents |
|---|---|---|
| `WordBattle.Core` | `Assets/Scripts/Core/` | Pure C# rules, namespace `WordBattle.Core`. `noEngineReferences: true`, so the compiler **forbids** `UnityEngine` here. |
| `WordBattle.Game` | `Assets/Scripts/Game/` (+ `UI/`) | MonoBehaviour adapters and views, namespaces `WordBattle.Game` and `WordBattle.Game.UI`. |
| `WordBattle.Editor` | `Assets/Editor/` | Editor-only tooling: `BattleSceneBuilder`. |
| `WordBattle.Tests.EditMode` | `Assets/Tests/EditMode/` | NUnit EditMode tests for Core, using a small in-memory word list (`TestWords`). Also holds `TestResultsWriter`, which writes every run's summary to `Temp/TestResults.txt`. |
| `WordBattle.Tests.PlayMode` | `Assets/Tests/PlayMode/` | `GameFlowTests`: load the real scene and click through Menu → Setup → 3 rounds → Victory → Play Again → Menu, plus tiles, Clear, Shuffle, and Lock In enabling. They fast-forward `Battle.Tick`. |

**Core (tested):**

| Type | Role |
|---|---|
| `LetterScoreTable`, `WordScorer` | Letter values and word score. Culture-invariant. |
| `WordDictionary` | Normalized A–Z word set, capped at `MaxWordLength` (8), with a words-by-length index. The real ENABLE1 file loads in about 45 ms: 80,368 words, including 28,420 eight-letter words. |
| `Rack`, `RackGenerator` | Immutable tiles with a multiset `CanForm`. The generator shuffles a random 8-letter seed word using an injected `System.Random`. |
| `WordValidator`, `WordStatus` | Returns `Empty`, `NotInRack`, `NotAWord`, `Valid`, or `Anagram`. This drives the red/green/gold colouring. |
| `WordSolver` | Every formable word for a rack, best first. Used by the AI and the "best possible" reveal. A real rack has about 100–200 words. |
| `Battle` | Shared rack and timer, driven by `Tick(dt)`.<br>`Submit(role, word)` returns a `SubmitResult`. **Invalid words are rejected without locking in.**<br>Each role locks in once.<br>`Submitted` carries only the role, so the word stays hidden. `Resolved` fires once.<br>Resolves when both players have submitted or time expires. |
| `BattleResult`, `Submission`, `BattleRole`, `WinReason`, `SubmitResult` | Result data. |
| `Match`, `MatchConfig` | Best-of-N (`RoundsToWin`). `Winner` is set *before* `RoundEnded` fires. |
| `AiRoster`, `AiDifficulty` | The built-in opponents, easiest first: **Pip** (Easy), **Marlow** (Medium), **Odessa** (Hard), **Vex** (Expert). Tests enforce that the names are unique, that none reuses an original Quarrel name, and that difficulty rises in order. |
| `IPlayerController` | The multiplayer seam. Implemented by:<br>• `HumanPlayerController`: words come from the UI.<br>• `AiPlayerController`: solves the rack and picks a word by skill percentile from an `AiProfile` (Name, Skill, SkillVariance, think-time range), then submits after its think time. |

**Game (Unity layer):**

| Type | Role |
|---|---|
| `DictionaryManager` | Singleton that loads the TextAsset into a `WordDictionary`. It persists across scenes, so it must be alone on a root GameObject. A duplicate destroys only its own component. |
| `BattleManager` | Holds the opponent roster (`opponents`, editable in the Inspector) and `SelectOpponent(i)`.<br>Owns a `Match` and both controllers.<br>Ticks the match from `Update`.<br>Re-raises `RoundStarted`, `PlayerLockedIn`, and `RoundEnded`.<br>`Continue()` starts the next round, or raises `MatchFinished` after the final results. |
| `GameModeManager`, `GameMode` | Toggles the MainMenu, GameSetup (opponent picker), Gameplay, and Victory screens.<br>`StartGame` opens setup, `StartMatch(i)` fights opponent *i*, `PlayAgain` rematches the same opponent. |
| `UI/MainMenuView`, `UI/SetupView`, `UI/BattleView`, `UI/VictoryView` | Views that render state on `OnEnable` and react to events. They contain no rules.<br>`BattleView` also handles:<br>• clickable tiles, which append a letter; used tiles are dimmed<br>• **Shuffle**, which reorders the display only, never the `Rack`<br>• **Clear** |

**Scene:** `Assets/Scenes/WordBattle.unity` is **generated** by *Word Battle → Rebuild Battle Scene* (`BattleSceneBuilder`). It contains:
- `[Systems]`: `BattleManager` and `GameModeManager`.
- `[Dictionary]`: `DictionaryManager`, alone on its own root because `DontDestroyOnLoad` persists the whole GameObject. Sharing it with other managers made them leak across scene reloads; the PlayMode tests caught this.
- `[UI] Canvas`: 1920×1080 reference resolution, Scale With Screen Size.
- `EventSystem`: uses `InputSystemUIInputModule`.

To change the layout, edit the builder and re-run it, rather than hand-editing the generated objects; a rebuild replaces them.

### Known issues and tech debt

1. **The project uses the new Input System only** (`activeInputHandler: 1`). Any EventSystem must use `InputSystemUIInputModule`, and scripts must not use `UnityEngine.Input`.
2. **Singleton access:** `DictionaryManager.Instance` is read when a match starts. This relies on the dictionary loading in `Awake`.
3. **Words longer than 8 letters are filtered at load time** (about 45 ms in the Editor). Check on mobile before adding an editor build step.
4. **The rack UI has a fixed 8 tiles,** built by the scene builder. `BattleView` logs an error if `RackSize` is larger.
5. **Default font:** TMP's Liberation Sans lacks symbols like ✓ ★ ✗, so UI copy uses plain text. Add a font with those glyphs if icons are wanted.
6. **Pre-release package:** `com.unity.ai.assistant` is pre-release (`2.20.0-pre.1`). It's an editor tool only.
7. **Editor testing caveat:** an unfocused Unity Editor doesn't advance Play mode frames, and may not process MCP commands at all. MCP-driven play tests advance `Battle.Tick` manually; real-time feel must be checked by a person.

## Still to build

- Polish: a Space-key shuffle shortcut, sounds, reveal animations, and opponent portraits (move the roster to ScriptableObjects when portraits arrive).
- Full game: territory, `PlayerTurnManager` and the turn timer, and 2–4 players.
