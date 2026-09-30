using System.Globalization;
using System.Threading;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class WordScorerTests
    {
        [TestCase("QUIZ", 31)]
        [TestCase("quiz", 31)]
        [TestCase("JAZZ", 40)]
        [TestCase("STRAINED", 11)]
        [TestCase("", 0)]
        [TestCase("   ", 0)]
        [TestCase(null, 0)]
        [TestCase("Q-U!", 18)]
        public void CalculateScore_SumsLetterValues(string word, int expected)
        {
            Assert.AreEqual(expected, WordScorer.CalculateScore(word));
        }

        [TestCase('A', 1)] [TestCase('B', 5)] [TestCase('C', 2)] [TestCase('D', 3)]
        [TestCase('E', 1)] [TestCase('F', 5)] [TestCase('G', 4)] [TestCase('H', 4)]
        [TestCase('I', 1)] [TestCase('J', 15)] [TestCase('K', 6)] [TestCase('L', 2)]
        [TestCase('M', 4)] [TestCase('N', 1)] [TestCase('O', 1)] [TestCase('P', 3)]
        [TestCase('Q', 15)] [TestCase('R', 2)] [TestCase('S', 1)] [TestCase('T', 1)]
        [TestCase('U', 3)] [TestCase('V', 6)] [TestCase('W', 5)] [TestCase('X', 10)]
        [TestCase('Y', 5)] [TestCase('Z', 12)]
        public void GetLetterScore_MatchesDesignTable(char letter, int expected)
        {
            Assert.AreEqual(expected, LetterScoreTable.GetLetterScore(letter));
            Assert.AreEqual(expected, LetterScoreTable.GetLetterScore(char.ToLowerInvariant(letter)));
        }

        [Test]
        public void GetLetterScore_NonLetter_IsZero()
        {
            Assert.AreEqual(0, LetterScoreTable.GetLetterScore('1'));
            Assert.AreEqual(0, LetterScoreTable.GetLetterScore('é'));
        }

        [Test]
        public void CalculateScore_IsCultureInvariant()
        {
            CultureInfo previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Assert.AreEqual(31, WordScorer.CalculateScore("quiz"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }
    }
}
