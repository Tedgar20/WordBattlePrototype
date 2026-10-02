using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordBattle.Core;

namespace WordBattle.Game.UI
{
    public class VictoryView : MonoBehaviour
    {
        [SerializeField] private GameModeManager gameModeManager;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        [SerializeField] private Color winColor = new Color(0.95f, 0.75f, 0.20f);
        [SerializeField] private Color loseColor = new Color(0.90f, 0.35f, 0.35f);

        private void OnEnable()
        {
            if (gameModeManager == null || battleManager == null || titleText == null || scoreText == null ||
                playAgainButton == null || mainMenuButton == null)
            {
                Debug.LogError("VictoryView: a reference is not assigned in the Inspector.", this);
                return;
            }

            playAgainButton.onClick.AddListener(gameModeManager.PlayAgain);
            mainMenuButton.onClick.AddListener(gameModeManager.ReturnToMainMenu);
            Render();
        }

        private void OnDisable()
        {
            if (playAgainButton != null && gameModeManager != null)
            {
                playAgainButton.onClick.RemoveListener(gameModeManager.PlayAgain);
                mainMenuButton.onClick.RemoveListener(gameModeManager.ReturnToMainMenu);
            }
        }

        private void Render()
        {
            Match match = battleManager.Match;
            if (match == null || !match.IsOver)
            {
                return;
            }

            bool humanWon = match.Winner == battleManager.HumanRole;
            string opponent = battleManager.OpponentProfile.Name;

            titleText.text = humanWon ? "Victory!" : "Defeat";
            titleText.color = humanWon ? winColor : loseColor;
            scoreText.text =
                $"You {match.GetWins(battleManager.HumanRole)} – {match.GetWins(battleManager.OpponentRole)} {opponent}";
        }
    }
}
