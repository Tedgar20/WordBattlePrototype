using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using WordBattle.Core;
using WordBattle.Game;

namespace WordBattle.Tests.PlayMode
{
    /// <summary>
    /// Smoke tests that load the real scene and drive it through its UI buttons.
    /// Battle time is fast-forwarded with Battle.Tick so tests don't wait 30 seconds a round.
    /// </summary>
    public class GameFlowTests
    {
        private const string Canvas = "[UI] Canvas/";
        private const string Gameplay = Canvas + "GameplayScreen/";

        private GameModeManager gameModeManager;
        private BattleManager battleManager;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Keep frames advancing even if the Editor window isn't focused.
            Application.runInBackground = true;

            SceneManager.LoadScene("WordBattle");
            yield return null; // Awake/OnEnable
            yield return null; // Start

            gameModeManager = Object.FindFirstObjectByType<GameModeManager>();
            battleManager = Object.FindFirstObjectByType<BattleManager>();
            Assert.IsNotNull(gameModeManager, "GameModeManager missing from scene");
            Assert.IsNotNull(battleManager, "BattleManager missing from scene");

            // Guards against systems leaking across scene loads via DontDestroyOnLoad.
            Assert.AreEqual(SceneManager.GetActiveScene(), gameModeManager.gameObject.scene,
                "GameModeManager should belong to the freshly loaded scene");
            Assert.AreEqual(1, Object.FindObjectsByType<GameModeManager>(FindObjectsSortMode.None).Length);
        }

        [UnityTest]
        public IEnumerator FullMatch_MenuToVictory_ThenPlayAgain_ThenMenu()
        {
            Assert.AreEqual(GameMode.MainMenu, gameModeManager.Mode);

            Click(Canvas + "MainMenuScreen/StartRow/StartButton");
            Assert.AreEqual(GameMode.GameSetup, gameModeManager.Mode);

            Click(Canvas + "SetupScreen/Opponents/OpponentCard1");
            Assert.AreEqual(GameMode.Gameplay, gameModeManager.Mode);
            Assert.AreEqual(battleManager.Opponents[0].Name, battleManager.OpponentProfile.Name);
            yield return null;

            // Win every round instantly with the best word: a tie with the AI goes to whoever locked in first.
            int guard = 0;
            while (!battleManager.Match.IsOver && guard++ < 5)
            {
                Battle battle = battleManager.CurrentBattle;
                Assert.AreEqual(SubmitResult.Accepted, battleManager.Human.Submit(BestWord(battle)));

                battle.Tick(battle.TimeLimit);
                yield return null;

                Assert.IsTrue(battle.IsResolved);
                Assert.IsTrue(Find(Gameplay + "ResultsPanel").activeSelf, "Results overlay should be showing");
                Click(Gameplay + "ResultsPanel/ContinueRow/ContinueButton");
                yield return null;
            }

            Assert.AreEqual(GameMode.Victory, gameModeManager.Mode);
            Assert.AreEqual("Victory!", Text(Canvas + "VictoryScreen/TitleText"));

            Click(Canvas + "VictoryScreen/Buttons/PlayAgainButton");
            yield return null;
            Assert.AreEqual(GameMode.Gameplay, gameModeManager.Mode);
            Assert.AreEqual(1, battleManager.Match.RoundNumber);
            Assert.AreEqual(0, battleManager.Match.AttackerWins + battleManager.Match.DefenderWins);
            Assert.IsFalse(Find(Gameplay + "ResultsPanel").activeSelf);

            gameModeManager.ReturnToMainMenu();
            Assert.AreEqual(GameMode.MainMenu, gameModeManager.Mode);
            Assert.IsNull(battleManager.Match);
        }

        [UnityTest]
        public IEnumerator SetupScreen_ShowsRoster_AndBackReturnsToMenu()
        {
            Click(Canvas + "MainMenuScreen/StartRow/StartButton");
            yield return null;

            for (int i = 0; i < battleManager.Opponents.Count; i++)
            {
                string card = $"{Canvas}SetupScreen/Opponents/OpponentCard{i + 1}/";
                Assert.AreEqual(battleManager.Opponents[i].Name, Text(card + "Name"));
            }

            Click(Canvas + "SetupScreen/BackRow/BackButton");
            Assert.AreEqual(GameMode.MainMenu, gameModeManager.Mode);
        }

        [UnityTest]
        public IEnumerator SelectingHarderOpponent_UsesThatProfile()
        {
            Click(Canvas + "MainMenuScreen/StartRow/StartButton");
            int last = battleManager.Opponents.Count;
            Click($"{Canvas}SetupScreen/Opponents/OpponentCard{last}");
            yield return null;

            Assert.AreEqual(battleManager.Opponents[last - 1].Name, battleManager.OpponentProfile.Name);
            StringAssert.Contains(battleManager.OpponentProfile.Name, Text(Gameplay + "Header/MatchScoreText"));
        }

        [UnityTest]
        public IEnumerator Tiles_BuildWord_ClearAndShuffle_AndOnlyValidWordsEnableLockIn()
        {
            Click(Canvas + "MainMenuScreen/StartRow/StartButton");
            Click(Canvas + "SetupScreen/Opponents/OpponentCard1");
            yield return null;

            var input = Find(Gameplay + "InputRow/WordInput").GetComponent<TMP_InputField>();
            var lockIn = Find(Gameplay + "SubmitRow/SubmitButton").GetComponent<Button>();
            Battle battle = battleManager.CurrentBattle;
            string word = BestWord(battle);

            // Click tiles to spell the word.
            var usedSlots = new HashSet<int>();
            foreach (char letter in word)
            {
                int slot = Enumerable.Range(0, battle.Rack.Size)
                    .First(i => !usedSlots.Contains(i) && Text($"{Gameplay}Rack/Tile{i + 1}/Letter") == letter.ToString());
                usedSlots.Add(slot);
                Click($"{Gameplay}Rack/Tile{slot + 1}");
            }

            Assert.AreEqual(word, input.text);
            Assert.IsTrue(lockIn.interactable, "A valid word should enable Lock In");

            Click(Gameplay + "SubmitRow/ClearButton");
            Assert.AreEqual(string.Empty, input.text);
            Assert.IsFalse(lockIn.interactable);

            input.text = "QQQQ";
            Assert.IsFalse(lockIn.interactable, "An invalid word must not enable Lock In");
            input.text = string.Empty;

            string before = SortedTileLetters(battle.Rack.Size);
            Click(Gameplay + "SubmitRow/ShuffleButton");
            Assert.AreEqual(before, SortedTileLetters(battle.Rack.Size), "Shuffle must keep the same letters");

            // Invalid words are rejected without locking in.
            Assert.AreEqual(SubmitResult.InvalidWord, battleManager.Human.Submit("QQQQ"));
            Assert.IsFalse(battle.HasSubmitted(battleManager.HumanRole));
        }

        private string SortedTileLetters(int count)
        {
            var letters = Enumerable.Range(1, count).Select(i => Text($"{Gameplay}Rack/Tile{i}/Letter")).ToList();
            letters.Sort(System.StringComparer.Ordinal);
            return string.Concat(letters);
        }

        private static string BestWord(Battle battle)
        {
            return WordSolver.FindWords(battle.Rack, DictionaryManager.Instance.Dictionary)[0];
        }

        private static GameObject Find(string path)
        {
            GameObject go = GameObject.Find(path);
            if (go == null)
            {
                var roots = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    roots.AddRange(scene.GetRootGameObjects().Select(r => $"{scene.name}:{r.name}"));
                }

                GameObject setup = GameObject.Find(Canvas + "SetupScreen");
                string setupChildren = setup == null
                    ? "SetupScreen inactive/missing"
                    : string.Join(", ", setup.GetComponentsInChildren<Transform>(true).Select(t => $"{t.name}({t.gameObject.activeSelf})"));
                Assert.Fail($"Couldn't find {path}. Roots: {string.Join(", ", roots)}. {setupChildren}");
            }
            return go;
        }

        private static void Click(string path)
        {
            var button = Find(path).GetComponent<Button>();
            Assert.IsTrue(button.interactable, $"{path} isn't clickable");
            button.onClick.Invoke();
        }

        private static string Text(string path)
        {
            return Find(path).GetComponent<TMP_Text>().text;
        }
    }
}
