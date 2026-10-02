namespace WordBattle.Core
{
    public enum SubmitResult
    {
        /// <summary>The word was valid and is now locked in.</summary>
        Accepted,

        /// <summary>The word isn't a dictionary word buildable from the rack. Nothing is locked in; the player can keep trying.</summary>
        InvalidWord,

        /// <summary>This player already locked in a word this battle.</summary>
        AlreadySubmitted,

        /// <summary>The battle has already resolved.</summary>
        BattleOver
    }
}
