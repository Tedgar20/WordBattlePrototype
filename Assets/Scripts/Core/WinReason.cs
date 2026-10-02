namespace WordBattle.Core
{
    public enum WinReason
    {
        /// <summary>The winner's word scored more points.</summary>
        HigherScore,

        /// <summary>Scores tied above zero; the winner locked in first.</summary>
        FasterSubmission,

        /// <summary>Neither player locked in a word, or both locked in equal scores at the same instant.</summary>
        DefenderByDefault
    }
}
