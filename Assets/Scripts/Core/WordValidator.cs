namespace WordBattle.Core
{
    public static class WordValidator
    {
        /// <summary>
        /// Checks a word against the rack and dictionary. Anagram means a valid word that uses every tile.
        /// </summary>
        public static WordStatus Validate(string word, Rack rack, WordDictionary dictionary)
        {
            string normalized = WordDictionary.Normalize(word);
            if (normalized.Length == 0)
            {
                return WordStatus.Empty;
            }

            if (!rack.CanForm(normalized))
            {
                return WordStatus.NotInRack;
            }

            if (!dictionary.Contains(normalized))
            {
                return WordStatus.NotAWord;
            }

            return normalized.Length == rack.Size ? WordStatus.Anagram : WordStatus.Valid;
        }

        public static bool IsScoring(WordStatus status)
        {
            return status == WordStatus.Valid || status == WordStatus.Anagram;
        }
    }
}
