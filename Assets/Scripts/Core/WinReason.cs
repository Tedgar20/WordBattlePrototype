namespace WordBattle.Core
{
    public enum WinReason
    {
        /// <summary>The winner's word scored more points.</summary>
        HigherScore,

        /// <summary>Scores tied above zero; the winner locked in first.</summary>
        FasterSubmission,

        /// <summary>Neither player scored (no valid words), or both locked in at the same instant.</summary>
        DefenderByDefault
    }
}
