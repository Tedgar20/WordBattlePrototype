using System;
using System.Linq;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class RackGeneratorTests
    {
        [Test]
        public void Generate_ProducesRackOfRequestedSizeFromASeedWord()
        {
            WordDictionary dictionary = TestWords.Dictionary();

            for (int seed = 0; seed < 50; seed++)
            {
                Rack rack = RackGenerator.Generate(dictionary, 8, new Random(seed));

                Assert.AreEqual(8, rack.Size);
                Assert.IsTrue(
                    dictionary.GetWordsOfLength(8).Any(rack.CanForm),
                    $"Rack {rack} can't form any 8-letter word");
            }
        }

        [Test]
        public void Generate_IsDeterministicForSameSeed()
        {
            WordDictionary dictionary = TestWords.Dictionary();

            Rack first = RackGenerator.Generate(dictionary, 8, new Random(42));
            Rack second = RackGenerator.Generate(dictionary, 8, new Random(42));

            Assert.AreEqual(first.ToString(), second.ToString());
        }

        [Test]
        public void Generate_ThrowsWhenNoWordsOfThatLength()
        {
            Assert.Throws<InvalidOperationException>(
                () => RackGenerator.Generate(TestWords.Dictionary(), 12, new Random(1)));
        }
    }
}
