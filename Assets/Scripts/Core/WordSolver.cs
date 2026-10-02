using System;
using System.Collections.Generic;

namespace WordBattle.Core
{
    public static class WordSolver
    {
        /// <summary>
        /// Every dictionary word that can be built from the rack, best first
        /// (highest score, then longest, then alphabetical).
        /// </summary>
        public static List<string> FindWords(Rack rack, WordDictionary dictionary)
        {
            if (rack == null)
            {
                throw new ArgumentNullException(nameof(rack));
            }

            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            var scored = new List<KeyValuePair<string, int>>();
            int maxLength = Math.Min(rack.Size, dictionary.MaxWordLength);

            for (int length = 1; length <= maxLength; length++)
            {
                foreach (string word in dictionary.GetWordsOfLength(length))
                {
                    if (rack.CanForm(word))
                    {
                        scored.Add(new KeyValuePair<string, int>(word, WordScorer.CalculateScore(word)));
                    }
                }
            }

            scored.Sort(CompareBestFirst);

            var results = new List<string>(scored.Count);
            foreach (KeyValuePair<string, int> entry in scored)
            {
                results.Add(entry.Key);
            }
            return results;
        }

        private static int CompareBestFirst(KeyValuePair<string, int> a, KeyValuePair<string, int> b)
        {
            int byScore = b.Value.CompareTo(a.Value);
            if (byScore != 0)
            {
                return byScore;
            }

            int byLength = b.Key.Length.CompareTo(a.Key.Length);
            return byLength != 0 ? byLength : string.CompareOrdinal(a.Key, b.Key);
        }
    }
}
