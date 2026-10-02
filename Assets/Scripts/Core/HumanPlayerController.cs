namespace WordBattle.Core
{
    /// <summary>A local player whose words come from the UI.</summary>
    public sealed class HumanPlayerController : IPlayerController
    {
        private Battle battle;

        public BattleRole Role { get; }

        public HumanPlayerController(BattleRole role)
        {
            Role = role;
        }

        public void BeginBattle(Battle battle)
        {
            this.battle = battle;
        }

        public void Tick()
        {
        }

        public WordStatus Validate(string word)
        {
            return battle?.Validate(word) ?? WordStatus.Empty;
        }

        public SubmitResult Submit(string word)
        {
            return battle?.Submit(Role, word) ?? SubmitResult.BattleOver;
        }
    }
}
