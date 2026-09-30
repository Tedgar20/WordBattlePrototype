namespace WordBattle.Core
{
    public sealed class BattleResult
    {
        /// <summary>Null if the attacker didn't submit before time ran out.</summary>
        public Submission Attacker { get; }

        /// <summary>Null if the defender didn't submit before time ran out.</summary>
        public Submission Defender { get; }

        public BattleRole Winner { get; }
        public WinReason Reason { get; }

        public BattleResult(Submission attacker, Submission defender, BattleRole winner, WinReason reason)
        {
            Attacker = attacker;
            Defender = defender;
            Winner = winner;
            Reason = reason;
        }

        public Submission GetSubmission(BattleRole role)
        {
            return role == BattleRole.Attacker ? Attacker : Defender;
        }

        public int GetScore(BattleRole role)
        {
            return GetSubmission(role)?.Score ?? 0;
        }
    }
}
