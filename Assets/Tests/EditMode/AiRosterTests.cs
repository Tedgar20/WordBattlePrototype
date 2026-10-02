using System;
using System.Linq;
using NUnit.Framework;
using WordBattle.Core;

namespace WordBattle.Tests
{
    public class AiRosterTests
    {
        // The original game's opponents. Ours must be legally distinct (see Docs/decisions.md D13).
        private static readonly string[] ForbiddenNames =
        {
            "Dwayne", "Caprice", "Biff", "Troy", "Damien", "Malik", "Rex", "Helena", "Kali"
        };

        [Test]
        public void Roster_HasUniqueOriginalNames()
        {
            AiProfile[] roster = AiRoster.CreateDefault();

            CollectionAssert.AllItemsAreUnique(roster.Select(p => p.Name));
            foreach (AiProfile profile in roster)
            {
                Assert.IsFalse(
                    ForbiddenNames.Contains(profile.Name, StringComparer.OrdinalIgnoreCase),
                    $"{profile.Name} is an original Quarrel character name");
                Assert.IsNotEmpty(profile.Tagline);
            }
        }

        [Test]
        public void Roster_GetsHarderInOrder()
        {
            AiProfile[] roster = AiRoster.CreateDefault();

            for (int i = 1; i < roster.Length; i++)
            {
                Assert.Greater(roster[i].Skill, roster[i - 1].Skill);
                Assert.Greater((int)roster[i].Difficulty, (int)roster[i - 1].Difficulty);
                Assert.LessOrEqual(roster[i].MaxThinkSeconds, roster[i - 1].MaxThinkSeconds);
            }
        }

        [Test]
        public void Roster_SettingsAreInRange()
        {
            foreach (AiProfile profile in AiRoster.CreateDefault())
            {
                Assert.That(profile.Skill, Is.InRange(0f, 1f), profile.Name);
                Assert.That(profile.SkillVariance, Is.InRange(0f, 0.5f), profile.Name);
                Assert.LessOrEqual(profile.MinThinkSeconds, profile.MaxThinkSeconds, profile.Name);
            }
        }

        [Test]
        public void CreateDefault_ReturnsFreshCopies()
        {
            AiProfile[] first = AiRoster.CreateDefault();
            first[0].Skill = 0f;

            Assert.AreNotEqual(0f, AiRoster.CreateDefault()[0].Skill);
        }
    }
}
