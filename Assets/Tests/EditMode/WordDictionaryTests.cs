using System.Globalization;
using System.Threading;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class WordDictionaryTests
    {
        [Test]
        public void Contains_IsCaseInsensitiveAndTrims()
        {
            WordDictionary dictionary = TestWords.Dictionary();

            Assert.IsTrue(dictionary.Contains("rain"));
            Assert.IsTrue(dictionary.Contains("RAIN"));
            Assert.IsTrue(dictionary.Contains("  Rain "));
            Assert.IsFalse(dictionary.Contains("rainz"));
            Assert.IsFalse(dictionary.Contains(""));
            Assert.IsFalse(dictionary.Contains(null));
        }

        [Test]
        public void WordsLongerThanMax_AreExcluded()
        {
            WordDictionary dictionary = TestWords.Dictionary();

            Assert.IsFalse(dictionary.Contains("abandoned"));
            Assert.AreEqual(8, dictionary.MaxWordLength);
        }

        [Test]
        public void FromText_HandlesCrlfBlankLinesAndDuplicates()
        {
            WordDictionary dictionary = WordDictionary.FromText("aa\r\nab\n\n  \nAA\nit's\n");

            Assert.AreEqual(2, dictionary.Count);
            Assert.IsTrue(dictionary.Contains("aa"));
            Assert.IsTrue(dictionary.Contains("ab"));
        }

        [Test]
        public void GetWordsOfLength_ReturnsOnlyThatLength()
        {
            WordDictionary dictionary = TestWords.Dictionary();

            CollectionAssert.AreEquivalent(new[] { "STRAINED", "DETRAINS" }, dictionary.GetWordsOfLength(8));
            Assert.IsEmpty(dictionary.GetWordsOfLength(12));
        }

        [Test]
        public void Contains_IsCultureInvariant()
        {
            CultureInfo previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Assert.IsTrue(TestWords.Dictionary().Contains("quiz"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }
    }
}
