using System;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class MatchTests
    {
        private static Match CreateMatch(int roundsToWin)
        {
            var config = new MatchConfig { RoundsToWin = roundsToWin, BattleTimeSeconds = 30f, RackSize = 8 };
            return new Match(config, TestWords.Dictionary(), new Random(7));
        }

        private static void PlayRound(Match match, BattleRole winner)
        {
            Battle battle = match.StartNextRound();
            BattleRole loser = winner == BattleRole.Attacker ? BattleRole.Defender : BattleRole.Attacker;
            battle.Submit(winner, AnyEightLetterWord(battle.Rack)); // 11 points
            battle.Submit(loser, "AN");                             // 2 points
        }

        private static string AnyEightLetterWord(Rack rack)
        {
            foreach (string word in TestWords.Dictionary().GetWordsOfLength(8))
            {
                if (rack.CanForm(word))
                {
                    return word;
                }
            }
            throw new InvalidOperationException($"No 8-letter word for rack {rack}");
        }

        [Test]
        public void BestOfThree_EndsAtTwoWins()
        {
            Match match = CreateMatch(2);
            BattleRole? matchWinner = null;
            match.MatchEnded += role => matchWinner = role;

            PlayRound(match, BattleRole.Attacker);
            Assert.IsFalse(match.IsOver);

            PlayRound(match, BattleRole.Defender);
            Assert.IsFalse(match.IsOver);

            PlayRound(match, BattleRole.Attacker);
            Assert.IsTrue(match.IsOver);
            Assert.AreEqual(BattleRole.Attacker, match.Winner);
            Assert.AreEqual(BattleRole.Attacker, matchWinner);
            Assert.AreEqual(3, match.RoundNumber);
            Assert.AreEqual(2, match.AttackerWins);
            Assert.AreEqual(1, match.DefenderWins);
        }

        [Test]
        public void BestOfThree_CanEndInTwoRounds()
        {
            Match match = CreateMatch(2);

            PlayRound(match, BattleRole.Defender);
            PlayRound(match, BattleRole.Defender);

            Assert.IsTrue(match.IsOver);
            Assert.AreEqual(BattleRole.Defender, match.Winner);
            Assert.AreEqual(2, match.RoundNumber);
        }

        [Test]
        public void RoundEnded_ForFinalRound_AlreadyReportsMatchOver()
        {
            Match match = CreateMatch(1);
            bool? isOverDuringRoundEnded = null;
            match.RoundEnded += _ => isOverDuringRoundEnded = match.IsOver;

            PlayRound(match, BattleRole.Attacker);

            Assert.IsTrue(isOverDuringRoundEnded);
        }

        [Test]
        public void BestOfOne_EndsAfterSingleBattle()
        {
            Match match = CreateMatch(1);

            PlayRound(match, BattleRole.Attacker);

            Assert.IsTrue(match.IsOver);
            Assert.AreEqual(BattleRole.Attacker, match.Winner);
        }

        [Test]
        public void Tick_ForwardsToBattleTimer()
        {
            Match match = CreateMatch(2);
            Battle battle = match.StartNextRound();

            match.Tick(30f);

            Assert.IsTrue(battle.IsResolved);
            Assert.AreEqual(1, match.DefenderWins); // nobody submitted → defender holds
        }

        [Test]
        public void CannotStartRound_WhileOneIsInProgress_OrAfterMatchEnds()
        {
            Match match = CreateMatch(1);
            match.StartNextRound();

            Assert.Throws<InvalidOperationException>(() => match.StartNextRound());

            match.Tick(30f);
            Assert.IsTrue(match.IsOver);
            Assert.Throws<InvalidOperationException>(() => match.StartNextRound());
        }

        [Test]
        public void EachRound_GetsAFreshRack()
        {
            Match match = CreateMatch(3);

            Battle first = match.StartNextRound();
            match.Tick(30f);
            Battle second = match.StartNextRound();

            Assert.AreNotSame(first, second);
            Assert.AreEqual(8, second.Rack.Size);
        }
    }
}
