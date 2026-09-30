using WordBattle.Core;

namespace WordBattle.Tests
{
    /// <summary>A tiny in-memory dictionary so tests don't depend on the 173k-word file.</summary>
    internal static class TestWords
    {
        public const string SeedWord = "STRAINED";

        public static WordDictionary Dictionary()
        {
            return new WordDictionary(new[]
            {
                "strained", "trained", "stained", "detrains", "sainted",
                "rain", "rains", "stain", "train", "diner", "tread", "dean",
                "an", "at", "quiz", "jazz",
                "abandoned" // 9 letters: must be filtered out
            });
        }

        public static Rack Rack()
        {
            return new Rack(SeedWord);
        }
    }
}
