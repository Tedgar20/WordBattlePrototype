using System;
using System.Collections.Generic;

namespace WordBattle.Core
{
    public static class RackGenerator
    {
        /// <summary>
        /// Builds a rack by shuffling the letters of a random dictionary word of the given size,
        /// so at least one word using every tile always exists.
        /// </summary>
        public static Rack Generate(WordDictionary dictionary, int size, Random random)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            IReadOnlyList<string> seeds = dictionary.GetWordsOfLength(size);
            if (seeds.Count == 0)
            {
                throw new InvalidOperationException($"The dictionary has no {size}-letter words to build a rack from.");
            }

            char[] letters = seeds[random.Next(seeds.Count)].ToCharArray();

            // Fisher–Yates shuffle.
            for (int i = letters.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (letters[i], letters[j]) = (letters[j], letters[i]);
            }

            return new Rack(letters);
        }
    }
}
