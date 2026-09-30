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

**Assemblies:**

| Assembly | Folder | Contents |
|---|---|---|
| `WordBattle.Core` | `Assets/Scripts/Core/` | Pure C# rules, namespace `WordBattle.Core`. `noEngineReferences: true`, so the compiler **forbids** `UnityEngine` here. |
| `WordBattle.Game` | `Assets/Scripts/Game/` | MonoBehaviour adapters, namespace `WordBattle.Game`. References Core, UGUI, and TMP. |
| `WordBattle.Tests.EditMode` | `Assets/Tests/EditMode/` | NUnit EditMode tests for Core, using a small in-memory word list (`TestWords`). |

**Core (done and tested):**

| Type | Role |
|---|---|
| `LetterScoreTable`, `WordScorer` | Letter values and word score. Culture-invariant. |
| `WordDictionary` | Normalized A–Z word set, capped at `MaxWordLength` (8), with a words-by-length index. `FromText` parses a TextAsset. The real ENABLE1 file loads in about 45 ms: 80,368 words, including 28,420 eight-letter words. |
| `Rack` | Immutable tiles. `CanForm(word)` is a multiset check (each tile used at most once). |
| `RackGenerator` | Shuffles a random 8-letter seed word using an injected `System.Random`. |
| `WordValidator` / `WordStatus` | Returns `Empty`, `NotInRack`, `NotAWord`, `Valid`, or `Anagram` (uses all tiles). This drives the red/green/gold colouring. |
| `Battle` | Shared rack and timer, driven by `Tick(dt)`.<br>`Submit(role, word)` locks in once; any word is accepted and invalid words score 0.<br>The `Submitted` event carries only the role, so words stay hidden until `Resolved`, which fires once.<br>Resolves when both players have submitted or time expires. |
| `BattleResult`, `Submission`, `BattleRole`, `WinReason` | Result data. `WinReason` is `HigherScore`, `FasterSubmission`, or `DefenderByDefault`. |
| `Match`, `MatchConfig` | Best-of-N over battles (`RoundsToWin`: 2 for the MVP, 1 in production). `StartNextRound()` is called explicitly by the caller. Raises `RoundStarted`, `RoundEnded`, and `MatchEnded`. |

**Game (Unity adapters):**

| File | Status |
|---|---|
| `DictionaryManager.cs` | ✅ Singleton that loads the TextAsset into a `WordDictionary`, exposed as `Dictionary`. |
| `GameStateManager.cs` | ⚠️ **Prototype with the wrong model** (alternating turns). Replace it with a `BattleManager` that drives `Match`, and delete it. |

### Known issues and tech debt

1. **The scene is empty.** `Assets/Scenes/WordBattle.unity` is still the untouched URP template (Main Camera + Global Light 2D).
   - The TMP UI and the `GameStateManager`/`DictionaryManager` objects described in earlier notes were **never saved**.
   - `Assets/_Recovery/0.unity` is an untracked crash-recovery scene with nothing worth keeping. It now also references the deleted `DebugTest` script. The user should delete it.
   - The UI must be rebuilt, ideally against the new Battle architecture rather than the prototype.
2. **The project uses the new Input System only** (`activeInputHandler: 1`).
   - Any EventSystem must use `InputSystemUIInputModule`, not `StandaloneInputModule`, which would throw errors.
   - Don't use `UnityEngine.Input` in scripts.
3. **Singleton access in `Start`:** `DictionaryManager.Instance` is used from other scripts' `Start`, which works only because the loading happens in `Awake`. Keep that invariant or use explicit initialization.
4. **Words longer than 8 letters are filtered at load time.** About 92k extra lines are parsed on every launch (about 45 ms total in the Editor). Check on mobile before adding an editor build step.
5. **Pre-release package:** `com.unity.ai.assistant` is pre-release (`2.20.0-pre.1`). It's an editor tool only, so it doesn't affect builds.

## Still to build (MVP)

```
BattleManager : MonoBehaviour   // adapter: owns a Match, ticks it from Update, bridges to UI & player controllers
IPlayerController               // HumanPlayerController (UI input), AiPlayerController (solver + think delay)
GameModeManager : MonoBehaviour // screens: MainMenu → GameSetup → Gameplay → Victory
Solver / AI word choice         // all dictionary words formable from a rack, ranked by score (word-engine)
```
