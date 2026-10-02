namespace WordBattle.Core
{
    public sealed class Submission
    {
        public BattleRole Role { get; }
        public string Word { get; }
        /// <summary>Valid or Anagram; invalid words are never locked in.</summary>
        public WordStatus Status { get; }

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
