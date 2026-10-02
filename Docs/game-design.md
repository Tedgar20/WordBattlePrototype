# Game Design

## Inspiration and legal distinctness

Quarrel (Denki / UTV Ignition) is a word-based strategy game: you invade neighbouring territory, then must out-score the defender with a word made from shared tiles. The original AI opponents (Dwayne → Kali, easiest → hardest) and 2–4 player matches are reference material.

**This game must be legally distinct.** Don't reuse the name "Quarrel", the original character names, art, or text. Mechanics and scoring ideas are fine. New AI opponent characters need original names and personalities.

## Letter scores

Values reflect how common a letter is in spoken, not written, English. Implemented in `LetterScoreTable.cs`.

| A 1 | B 5 | C 2 | D 3 | E 1 | F 5 | G 4 | H 4 | I 1 | J 15 | K 6 | L 2 | M 4 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **N 1** | **O 1** | **P 3** | **Q 15** | **R 2** | **S 1** | **T 1** | **U 3** | **V 6** | **W 5** | **X 10** | **Y 5** | **Z 12** |

A word's score is the sum of its letter values. Example: QUIZ = 15+3+1+12 = **31**.

## MVP spec (current target)

The goal is a small, playable demo of the **word battle** alone, with no territory or troops.

- **1v1:** a human vs a simple AI opponent. AI was chosen because a local human-vs-human game can't hide simultaneous input.
- **Best of 3 rounds.**
- **Shared tiles:** each round, both players get **the same 8 tiles**. Tiles come from a random 8-letter dictionary word, so at least one full anagram always exists.
- **Shared 30-second timer:** one global timer per round, not per player.
- **One submission per player:** once submitted, the player is locked in and can't change the word.
- **Round end:** the round ends when both players have submitted or the timer expires. Words are then revealed simultaneously.
- **Validation and scoring:** words must be in the dictionary and buildable from the rack. Score = sum of letter values.
- **Always a winner:** every battle produces a winner. Ties are broken by **fastest submission**: whoever locked in first wins. If neither player submitted, the **defender** wins.
- **Game flow:** Main Menu → Game Setup → Gameplay → Victory screen → Play Again / Return to Menu.

### Additional MVP features (backlog)

- **Validate as you type:** green = valid word, red = invalid, **gold = anagram** (uses all 8 tiles).
- **Dictionary as JSON** with words and definitions.
- **Dictionary limits:**
  - Generate a subset containing only the 8-letter words, used to seed racks.
  - Keep the dictionary free of words longer than 8 letters. There are currently 92,455 such words to strip and 28,420 eight-letter words.

## Full-game vision (post-MVP; don't build yet, but don't block it)

- **Players:** 2–4 per match, humans and/or AI (up to 3 AI opponents), with AI difficulty tiers.
- **Map:**
  - A territory map with troops, played in Risk-style turns.
  - On their turn, a player attacks adjacent territory, with each attack triggering a battle. They may attack repeatedly, and a lost battle does not end the turn.
  - A territory needs **at least 2 troops** to attack.
  - Players can take other players' troops.
  - The game is won by taking all territory.
- **Turn ending:** a turn ends on manual end-turn, when the turn timer expires, or when no valid attacks remain.
- **Timers:**
  - The **turn timer** runs during strategy and **pauses during a battle**.
  - The **battle timer** is 30s.
- **Battle format:** production battles are **best-of-1**, while the MVP uses best-of-3. The battle system must support both through configuration.
- **Platforms:** Windows, macOS, Linux, iOS, Android, Xbox, PlayStation, and Switch, with **cross-platform online multiplayer**. Keep logic deterministic and separate from UI and platform code.

## Maps and visual direction (full game)

**Maps: board-game style, entirely fictional.**
- Each map is a Risk-like board:
  - **territories** grouped into **regions** (continents)
  - a **region bonus** (extra troops) for holding a whole region
  - **adjacency** between neighbouring territories, including **sea routes** drawn as dashed lines between landmasses
- There will be **several maps**. They may differ in size, number of regions, and choke points, e.g. smaller maps for 2 players and larger ones for 4.
- **No map may represent the real world.** That rules out:
  - real continent or country silhouettes
  - real place names
  - recognisable real-world layouts
- Invent original lands, names, and themes (e.g. a sunken archipelago, a floating-island realm). This also keeps the game distinct from Risk and Quarrel.
- **Maps are data, not code.** A map asset defines:
  - its territories (id, name, region, board position/shape reference)
  - its regions (name, bonus)
  - the adjacency graph
  - starting troop rules

  Gameplay code only sees the graph. Visuals are a separate layer, so new maps can be added without code changes and tested in EditMode (e.g. "every territory is reachable").

**Visuals: semi-3D (2.5D).**
- The strategy board is shown in light 3D: a board viewed at an angle, with territories as raised or extruded low-poly regions, 3D troop tokens, and camera pan and zoom.
- The **word battle stays a 2D UI overlay**, as in the MVP.
- Art style is undecided; stylised and low-poly suits the scope.
- **Technical implication:** the project uses URP's **2D Renderer**, which doesn't light 3D meshes with 3D lights. Before board work starts, we'll add a Universal (3D) renderer for the board camera, or switch to it, while keeping the 2D UI. Decide this when the map feature starts.
- **Help available from Claude:** I can build the map data model, adjacency logic, camera controls, procedural meshes from territory outlines, materials, and placeholder art. Hand-crafted 3D models, textures, and illustrated maps need an artist, asset-store packs, or a separate generation tool. I can integrate and wire them up.

## Open questions

Ask the user before building anything that depends on one of these. Each lists a recommended default.

| # | Question | Recommended default |
|---|---|---|
| Q1 | Must the word use only rack letters, each at most as often as it appears? | Yes, Scrabble-style multiset check |
| Q2 | Does an invalid word, or no submission, score 0 or lose automatically? | ✅ **Decided: invalid words can't be submitted at all** (the player isn't locked in). No submission scores 0, and 0–0 goes to the defender (D17) |
| Q3 | Tie-break rule? | ✅ **Decided: fastest submission wins.** If neither player submitted, **the defender wins**, since the attacker chose to fight. In the MVP the AI is the defender |
| Q4 | Is there a bonus for using all 8 tiles (anagram)? | ✅ **Decided: no bonus.** Gold is only a visual highlight |
| Q5 | Minimum word length? | 2, matching the dictionary |
| Q6 | Should validation as you type reveal whether a word is valid, since that helps the player? | Yes in the MVP; could become a difficulty option |
| Q7 | How good is the MVP AI? | ✅ **Implemented:** opponent "Pip" (Skill 0.4 ± 0.15, 8–22s think time), tunable on `BattleManager` in the Inspector (D19) |
| Q8 | Is the opponent's word shown before the reveal? | No; only a "locked in" indicator |
| Q9 | Where do definitions for the JSON dictionary come from? | ✅ **Decided: the user will supply a definitions dictionary, committed to the repo.** JSON work waits for that file |
| Q10 | Which dictionary is the source? What is its licence? | ✅ **Decided: ENABLE1 (from GitHub).** ENABLE is public domain. Keep a note of the source URL in the repo |
| Q11 | How are maps made: hand-authored, procedurally generated, or both? | Hand-authored first (2–3 maps), using a simple map-data format; consider procedural later |
| Q12 | What are the rules and numbers for region bonuses, troop reinforcement, and capturing troops? | Spec with the `game-designer` agent when the map feature starts |
