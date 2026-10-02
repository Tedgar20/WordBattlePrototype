using UnityEngine;
using UnityEngine.UI;

namespace WordBattle.Game.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameModeManager gameModeManager;
        [SerializeField] private Button startButton;

        private void OnEnable()
        {
            if (gameModeManager == null || startButton == null)
            {
                Debug.LogError("MainMenuView: a reference is not assigned in the Inspector.", this);
                return;
            }

            startButton.onClick.AddListener(gameModeManager.StartGame);
        }

        private void OnDisable()
        {
            if (gameModeManager != null && startButton != null)
            {
                startButton.onClick.RemoveListener(gameModeManager.StartGame);
            }
        }
    }
}
