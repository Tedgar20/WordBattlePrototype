using System;
using System.Collections.Generic;
using UnityEngine;
using WordBattle.Core;

namespace WordBattle.Game
{
    /// <summary>
    /// Unity adapter for a Match: creates the players, ticks the battle timer from Update,
    /// and re-raises gameplay events for the UI. Holds no game rules itself.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private MatchConfig matchConfig = new MatchConfig();
        [Tooltip("Computer opponents, easiest first. The player picks one on the setup screen.")]
        [SerializeField] private AiProfile[] opponents = AiRoster.CreateDefault();
        [SerializeField] private int selectedOpponent;

        [Tooltip("0 = different racks every match. Any other value replays the same racks and AI choices.")]
        [SerializeField] private int randomSeed;

        [Tooltip("MVP: the human attacks and the AI defends.")]
        [SerializeField] private BattleRole humanRole = BattleRole.Attacker;

        private Match match;
        private AiPlayerController opponent;

        public Match Match => match;
        public HumanPlayerController Human { get; private set; }
        public IReadOnlyList<AiProfile> Opponents => opponents;
        public int SelectedOpponentIndex => selectedOpponent;
        public AiProfile OpponentProfile => opponents[selectedOpponent];
        public BattleRole HumanRole => humanRole;
        public BattleRole OpponentRole => humanRole == BattleRole.Attacker ? BattleRole.Defender : BattleRole.Attacker;
        public Battle CurrentBattle => match?.CurrentBattle;

        public event Action<Battle> RoundStarted;
        public event Action<BattleRole> PlayerLockedIn;
        public event Action<BattleResult> RoundEnded;

        /// <summary>Raised when the player continues past the final round's results.</summary>
        public event Action<BattleRole> MatchFinished;

        public void SelectOpponent(int index)
        {
            if (index < 0 || index >= opponents.Length)
            {
                Debug.LogError($"BattleManager: no opponent at index {index}.", this);
                return;
            }

            selectedOpponent = index;
        }

        public bool StartMatch()
        {
            if (opponents == null || opponents.Length == 0)
            {
                Debug.LogError("BattleManager: no opponents configured.", this);
                return false;
            }

            selectedOpponent = Mathf.Clamp(selectedOpponent, 0, opponents.Length - 1);

            WordDictionary dictionary = DictionaryManager.Instance != null ? DictionaryManager.Instance.Dictionary : null;
            if (dictionary == null)
            {
                Debug.LogError("BattleManager: no dictionary loaded. Is a DictionaryManager in the scene with its file assigned?", this);
                return false;
            }

            StopMatch();

            var random = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();
            match = new Match(matchConfig, dictionary, random);
            match.RoundEnded += OnRoundEnded;

            Human = new HumanPlayerController(humanRole);
            opponent = new AiPlayerController(OpponentRole, OpponentProfile, dictionary, random);

            StartNextRound();
            return true;
        }

        public void StopMatch()
        {
            if (match == null)
            {
                return;
            }

            match.RoundEnded -= OnRoundEnded;
            if (match.CurrentBattle != null)
            {
                match.CurrentBattle.Submitted -= OnSubmitted;
            }

            match = null;
            opponent = null;
            Human = null;
        }

        /// <summary>Called after the player has seen a round's results.</summary>
        public void Continue()
        {
            if (match == null || match.IsRoundInProgress)
            {
                return;
            }

            if (match.IsOver)
            {
                MatchFinished?.Invoke(match.Winner.Value);
            }
            else
            {
                StartNextRound();
            }
        }

        private void StartNextRound()
        {
            if (match.CurrentBattle != null)
            {
                match.CurrentBattle.Submitted -= OnSubmitted;
            }

            Battle battle = match.StartNextRound();
            battle.Submitted += OnSubmitted;

            Human.BeginBattle(battle);
            opponent.BeginBattle(battle);

            RoundStarted?.Invoke(battle);
        }

        private void Update()
        {
            if (match == null || !match.IsRoundInProgress)
            {
                return;
            }

            match.Tick(Time.deltaTime);
            opponent.Tick();
            Human.Tick();
        }

        private void OnSubmitted(BattleRole role)
        {
            PlayerLockedIn?.Invoke(role);
        }

        private void OnRoundEnded(BattleResult result)
        {
            RoundEnded?.Invoke(result);
        }
    }
}
