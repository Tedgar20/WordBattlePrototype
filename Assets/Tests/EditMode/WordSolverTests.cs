using System.Collections.Generic;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class WordSolverTests
    {
        [Test]
        public void FindWords_ReturnsEveryFormableWord()
        {
            List<string> words = WordSolver.FindWords(TestWords.Rack(), TestWords.Dictionary());

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "STRAINED", "DETRAINS", "TRAINED", "STAINED", "SAINTED",
                    "RAIN", "RAINS", "STAIN", "TRAIN", "DINER", "TREAD", "DEAN", "AN", "AT"
                },
                words);
        }

        [Test]
        public void FindWords_IsOrderedBestFirst()
        {
            List<string> words = WordSolver.FindWords(TestWords.Rack(), TestWords.Dictionary());

            for (int i = 1; i < words.Count; i++)
            {
                Assert.GreaterOrEqual(
                    WordScorer.CalculateScore(words[i - 1]),
                    WordScorer.CalculateScore(words[i]),
                    $"{words[i - 1]} should not rank below {words[i]}");
            }

            // DETRAINS and STRAINED tie at 11 points and 8 letters, so ordering falls back to alphabetical.
            Assert.AreEqual("DETRAINS", words[0]);
        }

        [Test]
        public void FindWords_EmptyWhenNothingFits()
        {
            Assert.IsEmpty(WordSolver.FindWords(new Rack("XXXX"), TestWords.Dictionary()));
        }
    }
}
