using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordBattle.Core;

namespace WordBattle.Game.UI
{
    /// <summary>Opponent picker: one card per AI in the BattleManager's roster.</summary>
    public class SetupView : MonoBehaviour
    {
        [SerializeField] private GameModeManager gameModeManager;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private Button[] opponentButtons;
        [SerializeField] private TMP_Text[] opponentNames;
        [SerializeField] private TMP_Text[] opponentDifficulties;
        [SerializeField] private TMP_Text[] opponentTaglines;
        [SerializeField] private Button backButton;

        [Header("Difficulty colours")]
        [SerializeField] private Color easyColor = new Color(0.30f, 0.78f, 0.47f);
        [SerializeField] private Color mediumColor = new Color(0.40f, 0.65f, 0.95f);
        [SerializeField] private Color hardColor = new Color(0.95f, 0.60f, 0.25f);
        [SerializeField] private Color expertColor = new Color(0.90f, 0.35f, 0.35f);

        private void OnEnable()
        {
            if (gameModeManager == null || battleManager == null || opponentButtons == null || backButton == null)
            {
                Debug.LogError("SetupView: a reference is not assigned in the Inspector.", this);
                return;
            }

            for (int i = 0; i < opponentButtons.Length; i++)
            {
                bool hasOpponent = i < battleManager.Opponents.Count;
                opponentButtons[i].gameObject.SetActive(hasOpponent);
                if (!hasOpponent)
                {
                    continue;
                }

                AiProfile profile = battleManager.Opponents[i];
                opponentNames[i].text = profile.Name;
                opponentDifficulties[i].text = profile.Difficulty.ToString().ToUpperInvariant();
                opponentDifficulties[i].color = ColorFor(profile.Difficulty);
                opponentTaglines[i].text = profile.Tagline;

                int index = i;
                opponentButtons[i].onClick.AddListener(() => gameModeManager.StartMatch(index));
            }

            backButton.onClick.AddListener(gameModeManager.ReturnToMainMenu);
        }

        private void OnDisable()
        {
            if (opponentButtons != null)
            {
                foreach (Button button in opponentButtons)
                {
                    button.onClick.RemoveAllListeners();
                }
            }

            if (backButton != null && gameModeManager != null)
            {
                backButton.onClick.RemoveListener(gameModeManager.ReturnToMainMenu);
            }
        }

        private Color ColorFor(AiDifficulty difficulty)
        {
            switch (difficulty)
            {
                case AiDifficulty.Easy: return easyColor;
                case AiDifficulty.Medium: return mediumColor;
                case AiDifficulty.Hard: return hardColor;
                default: return expertColor;
            }
        }
    }
}
