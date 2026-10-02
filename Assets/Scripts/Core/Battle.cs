using System;

namespace WordBattle.Core
{
    /// <summary>
    /// One word battle: both players share a rack and a single timer, each locks in one word,
    /// and the battle resolves when both have submitted or time runs out.
    /// Knows nothing about turns, territory, UI, or whether a player is human or AI.
    /// </summary>
    public sealed class Battle
    {
        private readonly WordDictionary dictionary;
        private Submission attackerSubmission;
        private Submission defenderSubmission;

        public Rack Rack { get; }
        public float TimeLimit { get; }
        public float Elapsed { get; private set; }
        public float TimeRemaining => Math.Max(0f, TimeLimit - Elapsed);
        public bool IsResolved => Result != null;
        public BattleResult Result { get; private set; }

        /// <summary>Raised when a player locks in. Carries only the role so the word stays hidden until the reveal.</summary>
        public event Action<BattleRole> Submitted;

        /// <summary>Raised once, when the battle resolves. Words are revealed here.</summary>
        public event Action<BattleResult> Resolved;

        public Battle(Rack rack, WordDictionary dictionary, float timeLimitSeconds)
        {
            Rack = rack ?? throw new ArgumentNullException(nameof(rack));
            this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

            if (timeLimitSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(timeLimitSeconds));
            }

            TimeLimit = timeLimitSeconds;
        }

        public bool HasSubmitted(BattleRole role)
        {
            return GetSubmission(role) != null;
        }

        /// <summary>Validates a word against this battle's rack and dictionary, e.g. for as-you-type feedback.</summary>
        public WordStatus Validate(string word)
        {
            return WordValidator.Validate(word, Rack, dictionary);
        }

        /// <summary>
        /// Locks in a word for the given player. Only valid words from the rack are accepted;
        /// an invalid word is rejected without locking the player in, so they can keep trying.
        /// </summary>
        public SubmitResult Submit(BattleRole role, string word)
        {
            if (IsResolved)
            {
                return SubmitResult.BattleOver;
            }

            if (HasSubmitted(role))
            {
                return SubmitResult.AlreadySubmitted;
            }

            string normalized = WordDictionary.Normalize(word);
            WordStatus status = Validate(normalized);
            if (!WordValidator.IsScoring(status))
            {
                return SubmitResult.InvalidWord;
            }

            var submission = new Submission(role, normalized, status, WordScorer.CalculateScore(normalized), Elapsed);

            if (role == BattleRole.Attacker)
            {
                attackerSubmission = submission;
            }
            else
            {
                defenderSubmission = submission;
            }

            Submitted?.Invoke(role);

            if (attackerSubmission != null && defenderSubmission != null)
            {
                Resolve();
            }

            return SubmitResult.Accepted;
        }

        public void Tick(float deltaTime)
        {
            if (IsResolved || deltaTime <= 0f)
            {
                return;
            }

            Elapsed = Math.Min(TimeLimit, Elapsed + deltaTime);

            if (Elapsed >= TimeLimit)
            {
                Resolve();
            }
        }

        private Submission GetSubmission(BattleRole role)
        {
            return role == BattleRole.Attacker ? attackerSubmission : defenderSubmission;
        }

        private void Resolve()
        {
            int attackerScore = attackerSubmission?.Score ?? 0;
            int defenderScore = defenderSubmission?.Score ?? 0;

            BattleRole winner;
            WinReason reason;

            if (attackerScore != defenderScore)
            {
                winner = attackerScore > defenderScore ? BattleRole.Attacker : BattleRole.Defender;
                reason = WinReason.HigherScore;
            }
            else if (attackerScore > 0 && attackerSubmission.SubmitTime != defenderSubmission.SubmitTime)
            {
                // Equal non-zero scores mean both locked in words: fastest submission wins.
                winner = attackerSubmission.SubmitTime < defenderSubmission.SubmitTime
                    ? BattleRole.Attacker
                    : BattleRole.Defender;
                reason = WinReason.FasterSubmission;
            }
            else
            {
                // Nobody submitted, or an exact tie: the attacker chose the fight, so the defender holds.
                winner = BattleRole.Defender;
                reason = WinReason.DefenderByDefault;
            }

            Result = new BattleResult(attackerSubmission, defenderSubmission, winner, reason);
            Resolved?.Invoke(Result);
        }
    }
}
