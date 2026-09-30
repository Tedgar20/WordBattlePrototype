using System.Collections.Generic;

namespace WordBattle.Core
{
    public static class LetterScoreTable
    {
        private static readonly Dictionary<char, int> Scores = new Dictionary<char, int>
        {
            // A–M
            {'A', 1}, {'B', 5}, {'C', 2}, {'D', 3},
            {'E', 1}, {'F', 5}, {'G', 4}, {'H', 4},
            {'I', 1}, {'J', 15}, {'K', 6}, {'L', 2},
            {'M', 4},

            // N–Z
            {'N', 1}, {'O', 1}, {'P', 3}, {'Q', 15},
            {'R', 2}, {'S', 1}, {'T', 1}, {'U', 3},
            {'V', 6}, {'W', 5}, {'X', 10}, {'Y', 5},
            {'Z', 12}
        };

        public static int GetLetterScore(char letter)
        {
            letter = char.ToUpperInvariant(letter);
            return Scores.TryGetValue(letter, out int score) ? score : 0;
        }
    }
}
