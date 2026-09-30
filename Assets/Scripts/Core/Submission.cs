namespace WordBattle.Core
{
    public sealed class Submission
    {
        public BattleRole Role { get; }
        public string Word { get; }
        public WordStatus Status { get; }

        /// <summary>Letter score of the word, or 0 if it isn't a valid word from the rack.</summary>
        public int Score { get; }

        /// <summary>Seconds since the battle started when the word was locked in.</summary>
        public float SubmitTime { get; }

        public Submission(BattleRole role, string word, WordStatus status, int score, float submitTime)
        {
            Role = role;
            Word = word;
            Status = status;
            Score = score;
            SubmitTime = submitTime;
        }
    }
}
