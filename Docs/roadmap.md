# Roadmap

Status key: ✅ done · 🟡 in progress · ⬜ not started

## MVP phases

| # | Phase | Status | Notes |
|---|---|---|---|
| 1 | Project setup and GitHub | ✅ | Unity 6.3 URP 2D, `.gitignore`, repo on GitHub |
| 2 | Scoring and dictionary | ✅ | `LetterScoreTable`, `WordScorer`, `DictionaryManager` (see known issues) |
| 3 | Game state and turn management | 🟡 | Prototype `GameStateManager` exists but uses the wrong model. Next: class diagram → GameModeManager → refactor state → Battle |
| 4 | Basic UI | 🟡 | TMP UI exists: PlayerText, TimerText, ScoreText, WordInputField, SubmitButton |
| 5 | Letter rack and tile system | ⬜ | Same 8 tiles for both players, seeded from 8-letter words |
| 6 | Simple AI opponent | ⬜ | Solver + difficulty knob + think delay |
| 7 | Best-of-3 rounds and 30s timer | ⬜ | Shared global battle timer, submission locking |
| 8 | Playable 1v1 demo | ⬜ | Menu → match → victory → replay |
| — | Unit testing | ⬜ | asmdefs + EditMode tests for scoring/validation/battle resolution |

## Recommended next steps (in order)

1. Fix quick wins:
   - rename `GameStateManaer.cs`
   - null-check the dictionary
   - route all scoring through `WordScorer`
2. Add asmdefs and an EditMode test project, with tests for `WordScorer` (QUIZ = 31) and dictionary validity.
3. Build `Rack`/`RackGenerator` and `WordValidator` (rack multiset check), with tests.
4. Build `Battle` (pure C#) and `BattleManager` (adapter): shared timer, lock-in, simultaneous reveal, resolve, tie-break.
5. Add `MatchController` (best-of-N) and `GameModeManager` (screens).
6. Add `AiPlayerController`.
7. UI pass: tile rack display, lock-in indicators, reveal, round results, victory screen.

## Feature backlog (post-core MVP)

- ⬜ Validate as you type: green valid / red invalid / gold anagram
- ⬜ Dictionary → JSON with definitions (waiting for the user to supply the definitions file)
- ⬜ 8-letter-word subset for rack seeding
- ⬜ Strip words longer than 8 letters from the dictionary

## Later (full game)

These come after the MVP:
- territory map and troops
- PlayerTurnManager and turn timer
- 2–4 players
- named AI opponents with difficulty tiers
- online cross-platform multiplayer
- platform ports
