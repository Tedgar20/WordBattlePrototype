using System.Collections.Generic;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class BattleTests
    {
        private Battle battle;

        [SetUp]
        public void SetUp()
        {
            battle = new Battle(TestWords.Rack(), TestWords.Dictionary(), 30f);
        }

        [Test]
        public void HigherScoreWins()
        {
            battle.Submit(BattleRole.Attacker, "STRAINED"); // 11
            battle.Submit(BattleRole.Defender, "RAIN");     // 5

            Assert.IsTrue(battle.IsResolved);
            Assert.AreEqual(BattleRole.Attacker, battle.Result.Winner);
            Assert.AreEqual(WinReason.HigherScore, battle.Result.Reason);
            Assert.AreEqual(11, battle.Result.GetScore(BattleRole.Attacker));
            Assert.AreEqual(5, battle.Result.GetScore(BattleRole.Defender));
        }

        [Test]
        public void ResolvesEarly_WhenBothSubmit()
        {
            battle.Tick(3f);
            battle.Submit(BattleRole.Defender, "RAIN");
            Assert.IsFalse(battle.IsResolved);

            battle.Submit(BattleRole.Attacker, "TRAIN");
            Assert.IsTrue(battle.IsResolved);
            Assert.AreEqual(3f, battle.Elapsed);
        }

        [Test]
        public void ResolvesWhenTimerExpires()
        {
            battle.Submit(BattleRole.Attacker, "RAIN");

            battle.Tick(29.9f);
            Assert.IsFalse(battle.IsResolved);

            battle.Tick(0.2f);
            Assert.IsTrue(battle.IsResolved);
            Assert.AreEqual(0f, battle.TimeRemaining);
            Assert.AreEqual(BattleRole.Attacker, battle.Result.Winner);
            Assert.IsNull(battle.Result.Defender);
        }

        [Test]
        public void SecondSubmissionIsRejected()
        {
            Assert.IsTrue(battle.Submit(BattleRole.Attacker, "RAIN"));
            Assert.IsFalse(battle.Submit(BattleRole.Attacker, "STRAINED"));

            battle.Submit(BattleRole.Defender, "AN");
            Assert.AreEqual("RAIN", battle.Result.Attacker.Word);
        }

        [Test]
        public void SubmissionAfterResolveIsRejected()
        {
            battle.Tick(30f);

            Assert.IsFalse(battle.Submit(BattleRole.Attacker, "RAIN"));
        }

        [Test]
        public void InvalidWord_ScoresZero()
        {
            battle.Submit(BattleRole.Attacker, "QUIZ");  // not in rack
            battle.Submit(BattleRole.Defender, "SNARE"); // not a word

            Assert.AreEqual(0, battle.Result.GetScore(BattleRole.Attacker));
            Assert.AreEqual(WordStatus.NotInRack, battle.Result.Attacker.Status);
            Assert.AreEqual(WordStatus.NotAWord, battle.Result.Defender.Status);
        }

        [Test]
        public void TiedScores_AttackerFaster_AttackerWins()
        {
            battle.Tick(1f);
            battle.Submit(BattleRole.Attacker, "TRAIN"); // T1 R2 A1 I1 N1 = 6 at 1s
            battle.Tick(4f);
            battle.Submit(BattleRole.Defender, "RAINS"); // R2 A1 I1 N1 S1 = 6 at 5s

            Assert.AreEqual(BattleRole.Attacker, battle.Result.Winner);
            Assert.AreEqual(WinReason.FasterSubmission, battle.Result.Reason);
        }

        [Test]
        public void TiedScores_DefenderFaster_DefenderWins()
        {
            battle.Tick(1f);
            battle.Submit(BattleRole.Defender, "TRAIN");
            battle.Tick(1f);
            battle.Submit(BattleRole.Attacker, "RAINS");

            Assert.AreEqual(BattleRole.Defender, battle.Result.Winner);
            Assert.AreEqual(WinReason.FasterSubmission, battle.Result.Reason);
        }

        [Test]
        public void TiedScores_SameInstant_DefenderWins()
        {
            battle.Submit(BattleRole.Attacker, "TRAIN");
            battle.Submit(BattleRole.Defender, "RAINS");

            Assert.AreEqual(BattleRole.Defender, battle.Result.Winner);
            Assert.AreEqual(WinReason.DefenderByDefault, battle.Result.Reason);
        }

        [Test]
        public void NobodySubmits_DefenderWins()
        {
            battle.Tick(30f);

            Assert.AreEqual(BattleRole.Defender, battle.Result.Winner);
            Assert.AreEqual(WinReason.DefenderByDefault, battle.Result.Reason);
        }

        [Test]
        public void BothInvalid_DefenderWinsEvenIfAttackerWasFaster()
        {
            battle.Submit(BattleRole.Attacker, "QUIZ");
            battle.Tick(5f);
            battle.Submit(BattleRole.Defender, "SNARE");

            Assert.AreEqual(BattleRole.Defender, battle.Result.Winner);
            Assert.AreEqual(WinReason.DefenderByDefault, battle.Result.Reason);
        }

        [Test]
        public void Events_SubmittedHidesWord_ResolvedFiresOnce()
        {
            var submittedRoles = new List<BattleRole>();
            int resolvedCount = 0;
            battle.Submitted += role => submittedRoles.Add(role);
            battle.Resolved += _ => resolvedCount++;

            battle.Submit(BattleRole.Defender, "RAIN");
            battle.Submit(BattleRole.Attacker, "TRAIN");
            battle.Tick(30f);

            CollectionAssert.AreEqual(new[] { BattleRole.Defender, BattleRole.Attacker }, submittedRoles);
            Assert.AreEqual(1, resolvedCount);
        }
    }
}
