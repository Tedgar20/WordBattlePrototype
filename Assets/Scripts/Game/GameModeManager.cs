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
        [SerializeField] private GameObject setupScreen;
        [SerializeField] private GameObject gameplayScreen;
        [SerializeField] private GameObject victoryScreen;

        public GameMode Mode { get; private set; }

        public event Action<GameMode> ModeChanged;

        private void Awake()
        {
            if (battleManager == null || mainMenuScreen == null || setupScreen == null || gameplayScreen == null ||
                victoryScreen == null)
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

        /// <summary>From the main menu: go to the setup screen to pick an opponent.</summary>
        public void StartGame()
        {
            SetMode(GameMode.GameSetup);
        }

        /// <summary>From the setup screen: fight the chosen opponent.</summary>
        public void StartMatch(int opponentIndex)
        {
            battleManager.SelectOpponent(opponentIndex);
            BeginMatch();
        }

        /// <summary>From the victory screen: rematch the same opponent.</summary>
        public void PlayAgain()
        {
            BeginMatch();
        }

        private void BeginMatch()
        {
            SetMode(battleManager.StartMatch() ? GameMode.Gameplay : GameMode.MainMenu);
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
            setupScreen.SetActive(mode == GameMode.GameSetup);
            gameplayScreen.SetActive(mode == GameMode.Gameplay);
            victoryScreen.SetActive(mode == GameMode.Victory);

            ModeChanged?.Invoke(mode);
        }
    }
}
