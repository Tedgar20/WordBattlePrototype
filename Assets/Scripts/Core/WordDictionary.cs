using System;
using System.Collections.Generic;

namespace WordBattle.Core
{
    /// <summary>
    /// The set of allowed words, normalized to uppercase A–Z and capped at a maximum length.
    /// </summary>
    public sealed class WordDictionary
    {
        private readonly HashSet<string> words = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<int, List<string>> wordsByLength = new Dictionary<int, List<string>>();

        public int MaxWordLength { get; }
        public int Count => words.Count;

        public WordDictionary(IEnumerable<string> rawWords, int maxWordLength = 8)
        {
            if (rawWords == null)
            {
                throw new ArgumentNullException(nameof(rawWords));
            }

            if (maxWordLength < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxWordLength));
            }

            MaxWordLength = maxWordLength;

            foreach (string raw in rawWords)
            {
                string word = Normalize(raw);
                if (word.Length == 0 || word.Length > maxWordLength || !IsAllLetters(word))
                {
                    continue;
                }

                if (words.Add(word))
                {
                    if (!wordsByLength.TryGetValue(word.Length, out List<string> list))
                    {
                        list = new List<string>();
                        wordsByLength[word.Length] = list;
                    }
                    list.Add(word);
                }
            }
        }

        /// <summary>Parses newline-separated text (LF or CRLF), e.g. the contents of a TextAsset.</summary>
        public static WordDictionary FromText(string text, int maxWordLength = 8)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return new WordDictionary(text.Split('\n'), maxWordLength);
        }

        public bool Contains(string word)
        {
            return words.Contains(Normalize(word));
        }

        public IReadOnlyList<string> GetWordsOfLength(int length)
        {
            return wordsByLength.TryGetValue(length, out List<string> list) ? list : (IReadOnlyList<string>)Array.Empty<string>();
        }

        public static string Normalize(string word)
        {
            return string.IsNullOrWhiteSpace(word) ? string.Empty : word.Trim().ToUpperInvariant();
        }

        private static bool IsAllLetters(string word)
        {
            foreach (char c in word)
            {
                if (c < 'A' || c > 'Z')
                {
                    return false;
                }
            }
            return true;
        }
    }
}
