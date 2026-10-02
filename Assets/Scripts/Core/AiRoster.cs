namespace WordBattle.Core
{
    /// <summary>The built-in computer opponents, easiest first. All names and personas are original.</summary>
    public static class AiRoster
    {
        public static AiProfile[] CreateDefault()
        {
            return new[]
            {
                new AiProfile
                {
                    Name = "Pip", Difficulty = AiDifficulty.Easy,
                    Tagline = "Cheerful, a little slow, happy with short words.",
                    Skill = 0.4f, SkillVariance = 0.15f, MinThinkSeconds = 8f, MaxThinkSeconds = 22f
                },
                new AiProfile
                {
                    Name = "Marlow", Difficulty = AiDifficulty.Medium,
                    Tagline = "Steady and sensible. Rarely misses a decent word.",
                    Skill = 0.65f, SkillVariance = 0.12f, MinThinkSeconds = 7f, MaxThinkSeconds = 18f
                },
                new AiProfile
                {
                    Name = "Odessa", Difficulty = AiDifficulty.Hard,
                    Tagline = "Sharp-eyed and quick. Hunts the high-value letters.",
                    Skill = 0.85f, SkillVariance = 0.08f, MinThinkSeconds = 5f, MaxThinkSeconds = 14f
                },
                new AiProfile
                {
                    Name = "Vex", Difficulty = AiDifficulty.Expert,
                    Tagline = "Ruthless. Usually finds the best word, and fast.",
                    Skill = 0.97f, SkillVariance = 0.03f, MinThinkSeconds = 4f, MaxThinkSeconds = 10f
                }
            };
        }
    }
}
