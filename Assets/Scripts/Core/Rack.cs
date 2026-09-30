using System;
using System.Collections.Generic;

namespace WordBattle.Core
{
    /// <summary>
    /// The letter tiles shared by both players in a battle. Immutable.
    /// </summary>
    public sealed class Rack
    {
        private readonly char[] tiles;
        private readonly int[] letterCounts = new int[26];

        public IReadOnlyList<char> Tiles => tiles;
        public int Size => tiles.Length;

        public Rack(IEnumerable<char> tiles)
        {
            if (tiles == null)
            {
                throw new ArgumentNullException(nameof(tiles));
            }

            var list = new List<char>();
            foreach (char raw in tiles)
            {
                char c = char.ToUpperInvariant(raw);
                if (c < 'A' || c > 'Z')
                {
                    throw new ArgumentException($"Rack tiles must be letters A–Z, got '{raw}'.", nameof(tiles));
                }
                list.Add(c);
                letterCounts[c - 'A']++;
            }

            this.tiles = list.ToArray();
        }

        /// <summary>
        /// True if the word can be spelled using each tile at most once.
        /// </summary>
        public bool CanForm(string word)
        {
            string normalized = WordDictionary.Normalize(word);
            if (normalized.Length == 0 || normalized.Length > tiles.Length)
            {
                return false;
            }

            int[] remaining = (int[])letterCounts.Clone();
            foreach (char c in normalized)
            {
                if (c < 'A' || c > 'Z')
                {
                    return false;
                }

                if (--remaining[c - 'A'] < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return new string(tiles);
        }
    }
}
