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

| File | Status |
|---|---|
| `Scripts/Core/LetterScoreTable.cs` | ✅ Static class; `GetLetterScore(char)` is culture-invariant, and unknown characters score 0. |
| `Scripts/Gameplay/WordScorer.cs` | ✅ Static class; `CalculateScore(string)` sums letter scores and skips non-letters. It is the single source of scoring. |
| `Scripts/Core/DictionaryManager.cs` | ✅ Singleton MonoBehaviour (`DontDestroyOnLoad`). Loads a `TextAsset` into an ordinal `HashSet`, keeps only words up to `maxWordLength` (8), null-checks the file, and has a null-safe `IsValidWord`. |
| `Scripts/Core/GameStateManager.cs` | ⚠️ **Prototype using the wrong model:** alternating turns with a per-player timer. Now hardened (Inspector ref checks, uses `WordScorer`), but it should be replaced by the layered design above. |
| `Scripts/Utils/DebugTest.cs` | 🧪 Scratch test for the QUIZ score; delete once real tests exist. |

### Known issues and tech debt

1. **The scene is empty.** `Assets/Scenes/WordBattle.unity` is still the untouched URP template (Main Camera + Global Light 2D).
   - The TMP UI and the `GameStateManager`/`DictionaryManager` objects described in earlier notes were **never saved**.
   - `Assets/_Recovery/0.unity`, a crash-recovery scene, holds only a DictionaryManager + DebugTest object.
   - The UI must be rebuilt, ideally against the new Battle architecture rather than the prototype.
2. **The project uses the new Input System only** (`activeInputHandler: 1`).
   - Any EventSystem must use `InputSystemUIInputModule`, not `StandaloneInputModule`, which would throw errors.
   - Don't use `UnityEngine.Input` in scripts.
3. **No rack check:** nothing verifies that a word is buildable from the tiles.
4. **Singleton access in `Start`:** `DictionaryManager.Instance` is used from other scripts' `Start`, which works only because the loading happens in `Awake`. Keep that invariant or use explicit initialization.
5. **Words longer than 8 letters are filtered at load time.** 92k extra words are still read on every launch; the editor build step from the roadmap would remove that cost.
6. **No tests:** there are no asmdefs, which EditMode tests require.
7. **Pre-release package:** `com.unity.ai.assistant` is pre-release (`2.20.0-pre.1`). It's an editor tool only, so it doesn't affect builds.

## Target class sketch (MVP)

```
GameModeManager : MonoBehaviour      // screen switching, Play Again / Menu
MatchController                      // plain C#: best-of-N over battles, match winner
Battle                               // plain C#: state, timer, submissions, Resolve() → BattleResult
BattleManager : MonoBehaviour        // adapter: ticks Battle, bridges to UI & players
IPlayerController                    // HumanPlayerController (UI), AiPlayerController (solver + think delay)
Rack / RackGenerator                 // 8 tiles from a random 8-letter word, shuffled
WordValidator                        // dictionary membership + rack multiset check
WordScorer, LetterScoreTable         // existing
WordDictionary                       // plain C# data: HashSet + anagram index; DictionaryManager loads it
```
