# Roadmap

Status key: ✅ done · 🟡 in progress · ⬜ not started

## MVP phases

| # | Phase | Status | Notes |
|---|---|---|---|
| 1 | Project setup and GitHub | ✅ | Unity 6.3 URP 2D, `.gitignore`, repo on GitHub |
| 2 | Scoring and dictionary | ✅ | `LetterScoreTable`, `WordScorer`, `DictionaryManager` (see known issues) |
| 3 | Game state and turn management | ✅ | `Battle`, `Match`, `BattleManager`, `GameModeManager`. (`PlayerTurnManager` is full-game only) |
| 4 | Basic UI | ✅ | Generated scene: menu, battle HUD, live validation, lock-in, results overlay, victory |
| 5 | Letter rack and tile system | ✅ | Logic and tile display (clickable tiles are polish) |
| 6 | Simple AI opponent | ✅ | `AiPlayerController` + `AiProfile` ("Pip") |
| 7 | Best-of-3 rounds and 30s timer | ✅ | Shared timer, lock-in, simultaneous reveal |
| 8 | Playable 1v1 demo | ✅ | Play-tested by the user: 30s feels right, and Pip reads as Easy |
| — | Unit testing | ✅ | asmdefs + EditMode tests for all Core logic (solver, AI, and controllers included) |

## Recommended next steps (in order)

1. ✅ Clickable tiles, Shuffle, and Clear
2. ✅ Opponent picker with 4 original AI opponents (Pip Easy → Vex Expert)
3. ✅ PlayMode smoke tests for the full flow
4. The user play-tests Marlow, Odessa, and Vex and tunes `AiRoster` values.
5. Polish: Space to shuffle, sounds, reveal animation.
6. Full game: spec the map data model with `game-designer`, then the map data and board (see `game-design.md` → Maps and visual direction).

## Feature backlog (post-core MVP)

- ✅ Validate as you type: green valid / red invalid / gold anagram (with a text cue too)
- ⬜ Dictionary → JSON with definitions (waiting for the user to supply the definitions file)
- ✅ 8-letter-word subset for rack seeding (`WordDictionary.GetWordsOfLength(8)`)
- ✅ Words longer than 8 letters are filtered at load (an editor build step to strip them from the file is optional; load is about 45 ms)

## Later (full game)

These come after the MVP:
- map data model (territories, regions, adjacency, sea routes) + 2–3 fictional maps
- semi-3D board: tilted camera, extruded territories, troop tokens (add a 3D renderer first)
- troops, region bonuses, reinforcement
- PlayerTurnManager and turn timer
- 2–4 players
- named AI opponents with difficulty tiers
- online cross-platform multiplayer
- platform ports
