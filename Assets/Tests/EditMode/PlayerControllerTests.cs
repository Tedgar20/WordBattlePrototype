using System;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class PlayerControllerTests
    {
        private static Battle NewBattle()
        {
            return new Battle(TestWords.Rack(), TestWords.Dictionary(), 30f);
        }

        private static AiPlayerController NewAi(float skill, float minThink = 5f, float maxThink = 5f, int seed = 1)
        {
            var profile = new AiProfile
            {
                Name = "Test",
                Skill = skill,
                SkillVariance = 0f,
                MinThinkSeconds = minThink,
                MaxThinkSeconds = maxThink
            };
            return new AiPlayerController(BattleRole.Defender, profile, TestWords.Dictionary(), new Random(seed));
        }

        [Test]
        public void Ai_MaxSkill_PlaysBestWord()
        {
            AiPlayerController ai = NewAi(1f);
            ai.BeginBattle(NewBattle());

            Assert.AreEqual("DETRAINS", ai.PlannedWord);
        }

        [Test]
        public void Ai_MinSkill_PlaysWeakestWord()
        {
            AiPlayerController ai = NewAi(0f);
            ai.BeginBattle(NewBattle());

            Assert.AreEqual(WordScorer.CalculateScore("AN"), WordScorer.CalculateScore(ai.PlannedWord));
        }

        [Test]
        public void Ai_SubmitsOnlyAfterThinkTime()
        {
            Battle battle = NewBattle();
            AiPlayerController ai = NewAi(1f, 5f, 5f);
            ai.BeginBattle(battle);

            battle.Tick(4.9f);
            ai.Tick();
            Assert.IsFalse(battle.HasSubmitted(BattleRole.Defender));

            battle.Tick(0.2f);
            ai.Tick();
            Assert.IsTrue(battle.HasSubmitted(BattleRole.Defender));
        }

        [Test]
        public void Ai_ThinkTime_IsCappedBeforeTimeLimit()
        {
            AiPlayerController ai = NewAi(1f, 100f, 200f);
            ai.BeginBattle(NewBattle());

            Assert.Less(ai.PlannedSubmitTime, 30f);
        }

        [Test]
        public void Ai_PlannedWord_IsAlwaysValidForRack()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var profile = new AiProfile { Skill = 0.5f, SkillVariance = 0.5f };
                var ai = new AiPlayerController(BattleRole.Defender, profile, TestWords.Dictionary(), new Random(seed));
                Battle battle = NewBattle();
                ai.BeginBattle(battle);

                Assert.IsTrue(WordValidator.IsScoring(battle.Validate(ai.PlannedWord)), ai.PlannedWord);
            }
        }

        [Test]
        public void Ai_NoFormableWords_NeverSubmits()
        {
            var battle = new Battle(new Rack("XXXX"), TestWords.Dictionary(), 30f);
            AiPlayerController ai = NewAi(1f);
            ai.BeginBattle(battle);

            battle.Tick(29f);
            ai.Tick();

            Assert.IsNull(ai.PlannedWord);
            Assert.IsFalse(battle.HasSubmitted(BattleRole.Defender));
        }

        [Test]
        public void Human_SubmitsForItsRole()
        {
            Battle battle = NewBattle();
            var human = new HumanPlayerController(BattleRole.Attacker);
            human.BeginBattle(battle);

            Assert.AreEqual(WordStatus.Anagram, human.Validate("strained"));
            Assert.AreEqual(SubmitResult.InvalidWord, human.Submit("QUIZ"));
            Assert.AreEqual(SubmitResult.Accepted, human.Submit("RAIN"));
            Assert.IsTrue(battle.HasSubmitted(BattleRole.Attacker));
        }

        [Test]
        public void Human_BeforeAnyBattle_IsSafe()
        {
            var human = new HumanPlayerController(BattleRole.Attacker);

            Assert.AreEqual(WordStatus.Empty, human.Validate("RAIN"));
            Assert.AreEqual(SubmitResult.BattleOver, human.Submit("RAIN"));
        }
    }
}
