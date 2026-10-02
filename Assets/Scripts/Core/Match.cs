using System;

namespace WordBattle.Core
{
    /// <summary>
    /// A best-of-N series of battles between the same attacker and defender.
    /// Rounds are started explicitly so the caller controls pacing (e.g. showing results between rounds).
    /// </summary>
    public sealed class Match
    {
        private readonly MatchConfig config;
        private readonly WordDictionary dictionary;
        private readonly Random random;

        public int AttackerWins { get; private set; }
        public int DefenderWins { get; private set; }
        public int RoundNumber { get; private set; }
        public Battle CurrentBattle { get; private set; }
        public bool IsRoundInProgress => CurrentBattle != null && !CurrentBattle.IsResolved;
        public bool IsOver => Winner.HasValue;
        public BattleRole? Winner { get; private set; }
        public int RoundsToWin => config.RoundsToWin;

        public event Action<Battle> RoundStarted;
        public event Action<BattleResult> RoundEnded;
        public event Action<BattleRole> MatchEnded;

        public Match(MatchConfig config, WordDictionary dictionary, Random random)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            this.random = random ?? throw new ArgumentNullException(nameof(random));

            if (config.RoundsToWin < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(config), "RoundsToWin must be at least 1.");
            }
        }

        public int GetWins(BattleRole role)
        {
            return role == BattleRole.Attacker ? AttackerWins : DefenderWins;
        }

        public Battle StartNextRound()
        {
            if (IsOver)
            {
                throw new InvalidOperationException("The match is already over.");
            }

            if (IsRoundInProgress)
            {
                throw new InvalidOperationException("The current round hasn't finished yet.");
            }

            if (CurrentBattle != null)
            {
                CurrentBattle.Resolved -= OnBattleResolved;
            }

            Rack rack = RackGenerator.Generate(dictionary, config.RackSize, random);
            CurrentBattle = new Battle(rack, dictionary, config.BattleTimeSeconds);
            CurrentBattle.Resolved += OnBattleResolved;
            RoundNumber++;

            RoundStarted?.Invoke(CurrentBattle);
            return CurrentBattle;
        }

        public void Tick(float deltaTime)
        {
            if (IsRoundInProgress)
            {
                CurrentBattle.Tick(deltaTime);
            }
        }

        private void OnBattleResolved(BattleResult result)
        {
            if (result.Winner == BattleRole.Attacker)
            {
                AttackerWins++;
            }
            else
            {
                DefenderWins++;
            }

            // Decide the match before notifying, so RoundEnded listeners already see IsOver for the final round.
            if (AttackerWins >= config.RoundsToWin || DefenderWins >= config.RoundsToWin)
            {
                Winner = AttackerWins >= config.RoundsToWin ? BattleRole.Attacker : BattleRole.Defender;
            }

            RoundEnded?.Invoke(result);

            if (Winner.HasValue)
            {
                MatchEnded?.Invoke(Winner.Value);
            }
        }
    }
}
