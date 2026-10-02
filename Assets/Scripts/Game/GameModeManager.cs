using System;
using UnityEngine;

namespace WordBattle.Game
{
    /// <summary>
    /// Top-level screen flow: MainMenu → GameSetup → Gameplay → Victory → (Play Again | Main Menu).
    /// </summary>
    public class GameModeManager : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;

        [Header("Screens")]
        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject gameplayScreen;
        [SerializeField] private GameObject victoryScreen;

        public GameMode Mode { get; private set; }

        public event Action<GameMode> ModeChanged;

        private void Awake()
        {
            if (battleManager == null || mainMenuScreen == null || gameplayScreen == null || victoryScreen == null)
            {
                Debug.LogError("GameModeManager: a reference is not assigned in the Inspector.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.MatchFinished += OnMatchFinished;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.MatchFinished -= OnMatchFinished;
            }
        }

        private void Start()
        {
            SetMode(GameMode.MainMenu);
        }

        public void StartGame()
        {
            // MVP has no setup options yet (opponent choice comes later), so setup is instant.
            SetMode(GameMode.GameSetup);

            if (battleManager.StartMatch())
            {
                SetMode(GameMode.Gameplay);
            }
            else
            {
                SetMode(GameMode.MainMenu);
            }
        }

        public void PlayAgain()
        {
            StartGame();
        }

        public void ReturnToMainMenu()
        {
            battleManager.StopMatch();
            SetMode(GameMode.MainMenu);
        }

        private void OnMatchFinished(Core.BattleRole winner)
        {
            SetMode(GameMode.Victory);
        }

        private void SetMode(GameMode mode)
        {
            Mode = mode;

            mainMenuScreen.SetActive(mode == GameMode.MainMenu);
            gameplayScreen.SetActive(mode == GameMode.Gameplay);
            victoryScreen.SetActive(mode == GameMode.Victory);

            ModeChanged?.Invoke(mode);
        }
    }
}
