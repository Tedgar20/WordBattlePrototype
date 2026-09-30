using System;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class RackTests
    {
        [TestCase("STRAINED", true)]
        [TestCase("rain", true)]
        [TestCase("DETRAINS", true)]
        [TestCase("TREATS", false)]    // needs two Ts
        [TestCase("QUIZ", false)]      // letters not in rack
        [TestCase("STRAINEDS", false)] // longer than rack
        [TestCase("", false)]
        [TestCase(null, false)]
        [TestCase("RA-IN", false)]
        public void CanForm_UsesEachTileAtMostOnce(string word, bool expected)
        {
            Assert.AreEqual(expected, TestWords.Rack().CanForm(word));
        }

        [Test]
        public void CanForm_AllowsRepeatedLettersWhenRackHasThem()
        {
            var rack = new Rack("BALLOONS");

            Assert.IsTrue(rack.CanForm("BALLOON"));
            Assert.IsTrue(rack.CanForm("LOON"));
            Assert.IsFalse(rack.CanForm("LOLL"));
        }

        [Test]
        public void Constructor_NormalizesToUppercase()
        {
            var rack = new Rack("abc");

            Assert.AreEqual("ABC", rack.ToString());
            Assert.AreEqual(3, rack.Size);
        }

        [Test]
        public void Constructor_RejectsNonLetters()
        {
            Assert.Throws<ArgumentException>(() => new Rack("AB1"));
        }
    }
}
