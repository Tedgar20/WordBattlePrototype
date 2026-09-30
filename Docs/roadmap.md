# Roadmap

Status key: ✅ done · 🟡 in progress · ⬜ not started

## MVP phases

| # | Phase | Status | Notes |
|---|---|---|---|
| 1 | Project setup and GitHub | ✅ | Unity 6.3 URP 2D, `.gitignore`, repo on GitHub |
| 2 | Scoring and dictionary | ✅ | `LetterScoreTable`, `WordScorer`, `DictionaryManager` (see known issues) |
| 3 | Game state and turn management | 🟡 | Core `Battle` and `Match` (best-of-N) are done and tested. Next: `BattleManager` adapter, then `GameModeManager`; retire the prototype `GameStateManager` |
| 4 | Basic UI | ⬜ | A TMP UI was prototyped but **never saved to the scene**; rebuild it against the Battle architecture |
| 5 | Letter rack and tile system | 🟡 | Logic done (`Rack`, `RackGenerator`, `WordValidator`); tile UI still to build |
| 6 | Simple AI opponent | ⬜ | Solver + difficulty knob + think delay |
| 7 | Best-of-3 rounds and 30s timer | 🟡 | Logic done in `Battle`/`Match`; needs wiring to the UI |
| 8 | Playable 1v1 demo | ⬜ | Menu → match → victory → replay |
| — | Unit testing | ✅ | asmdefs + 88 EditMode tests (scoring, dictionary, rack, validator, battle, match) |

## Recommended next steps (in order)

1. ✅ Quick wins: file rename, dictionary null check, all scoring through `WordScorer`, culture-invariant casing, ≤8-letter filter at load
2. ✅ asmdefs + EditMode tests
3. ✅ `Rack`, `RackGenerator`, `WordValidator`
4. ✅ `Battle` and `Match` (pure C#). ⬜ `BattleManager` adapter and player controllers
5. ⬜ `GameModeManager` (screens)
6. Add `AiPlayerController`.
7. UI pass: tile rack display, lock-in indicators, reveal, round results, victory screen.

## Feature backlog (post-core MVP)

- ⬜ Validate as you type: green valid / red invalid / gold anagram
- ⬜ Dictionary → JSON with definitions (waiting for the user to supply the definitions file)
- ✅ 8-letter-word subset for rack seeding (`WordDictionary.GetWordsOfLength(8)`)
- ✅ Words longer than 8 letters are filtered at load (an editor build step to strip them from the file is optional; load is about 45 ms)

## Later (full game)

These come after the MVP:
- territory map and troops
- PlayerTurnManager and turn timer
- 2–4 players
- named AI opponents with difficulty tiers
- online cross-platform multiplayer
- platform ports
