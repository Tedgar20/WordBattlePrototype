using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class WordValidatorTests
    {
        [TestCase("rain", WordStatus.Valid)]
        [TestCase("STRAINED", WordStatus.Anagram)]
        [TestCase("detrains", WordStatus.Anagram)]
        [TestCase("SNARE", WordStatus.NotAWord)]   // formable, but not in the test dictionary
        [TestCase("QUIZ", WordStatus.NotInRack)]   // real word, wrong letters
        [TestCase("TREATS", WordStatus.NotInRack)] // needs a second T
        [TestCase("", WordStatus.Empty)]
        [TestCase("  ", WordStatus.Empty)]
        [TestCase(null, WordStatus.Empty)]
        public void Validate_ClassifiesWord(string word, WordStatus expected)
        {
            Assert.AreEqual(expected, WordValidator.Validate(word, TestWords.Rack(), TestWords.Dictionary()));
        }

        [TestCase(WordStatus.Valid, true)]
        [TestCase(WordStatus.Anagram, true)]
        [TestCase(WordStatus.NotAWord, false)]
        [TestCase(WordStatus.NotInRack, false)]
        [TestCase(WordStatus.Empty, false)]
        public void IsScoring(WordStatus status, bool expected)
        {
            Assert.AreEqual(expected, WordValidator.IsScoring(status));
        }
    }
}
