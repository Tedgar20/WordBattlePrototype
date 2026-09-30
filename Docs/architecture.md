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
| `Scripts/Core/LetterScoreTable.cs` | ✅ Static score table plus `GetLetterScore(char)`; unknown characters → 0. Fine. |
| `Scripts/Gameplay/WordScorer.cs` | ✅ `CalculateScore(string)` sums letter scores and skips non-letters. |
| `Scripts/Core/DictionaryManager.cs` | ⚠️ A singleton MonoBehaviour (`DontDestroyOnLoad`) that loads a `TextAsset` into a `HashSet`, with `IsValidWord`. It doesn't null-check `dictionaryFile`, and it doesn't filter out words longer than 8 letters. |
| `Scripts/Core/GameStateManaer.cs` | ⚠️ An **early prototype** using the wrong model: alternating turns with a per-player timer. It should be replaced by the layered design above. |
| `Scripts/Utils/DebugTest.cs` | 🧪 Scratch test for the QUIZ score; can be deleted once real tests exist. |

### Known issues and tech debt

1. **`GameStateManaer.cs` filename typo.** The class is `GameStateManager`. Unity requires the file name to match a MonoBehaviour's class name, or the component can't be added or loaded. Rename the file together with its `.meta`. Doing it in the Editor, or with `git mv` for both files, preserves the GUID.
2. **Duplicate scoring logic:** `GameStateManager.CalculateWordScore` duplicates `WordScorer`.
3. **No rack check:** nothing verifies that a word is buildable from the tiles.
4. **Singleton access in `Start`:** `DictionaryManager.Instance` is used from other scripts' `Start`, which works only because the loading happens in `Awake`. Keep that invariant or use explicit initialization.
5. **Dictionary not filtered:** the dictionary contains words longer than 8 letters. Filter them at load time or with an editor build step.
6. **Stray scene:** `Assets/_Recovery/0.unity` is a crash-recovery scene and is probably safe to delete. Confirm with the user first.
7. **No tests:** there are no asmdefs, which EditMode tests require.

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
