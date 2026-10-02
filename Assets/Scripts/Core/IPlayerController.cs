namespace WordBattle.Core
{
    /// <summary>
    /// One side of a battle. Human, AI, and (later) remote players all implement this,
    /// so Battle and the managers never need to know which kind they're dealing with.
    /// </summary>
    public interface IPlayerController
    {
        BattleRole Role { get; }

        /// <summary>Called when a new battle starts.</summary>
        void BeginBattle(Battle battle);

        /// <summary>Called every frame while the battle is in progress.</summary>
        void Tick();
    }
}
