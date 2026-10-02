using System;
using System.Collections.Generic;

namespace WordBattle.Core
{
    /// <summary>
    /// A computer opponent. At the start of each battle it solves the rack, picks a word
    /// according to its skill, and locks it in after a simulated think time.
    /// </summary>
    public sealed class AiPlayerController : IPlayerController
    {
        // Leave the human a moment at the end so the AI never submits on the final frame.
        private const float LatestSubmitFraction = 0.95f;

        private readonly AiProfile profile;
        private readonly WordDictionary dictionary;
        private readonly Random random;
        private Battle battle;

        public BattleRole Role { get; }
        public AiProfile Profile => profile;

        /// <summary>The word the AI intends to play this battle, or null if the rack has no words.</summary>
        public string PlannedWord { get; private set; }

        public float PlannedSubmitTime { get; private set; }

        public AiPlayerController(BattleRole role, AiProfile profile, WordDictionary dictionary, Random random)
        {
            Role = role;
            this.profile = profile ?? throw new ArgumentNullException(nameof(profile));
            this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public void BeginBattle(Battle battle)
        {
            this.battle = battle ?? throw new ArgumentNullException(nameof(battle));

            List<string> candidates = WordSolver.FindWords(battle.Rack, dictionary);
            PlannedWord = candidates.Count > 0 ? ChooseWord(candidates) : null;
            PlannedSubmitTime = ChooseSubmitTime(battle.TimeLimit);
        }

        public void Tick()
        {
            if (battle == null || battle.IsResolved || PlannedWord == null || battle.HasSubmitted(Role))
            {
                return;
            }

            if (battle.Elapsed >= PlannedSubmitTime)
            {
                battle.Submit(Role, PlannedWord);
            }
        }

        private string ChooseWord(List<string> bestFirst)
        {
            double wobble = (random.NextDouble() * 2.0 - 1.0) * profile.SkillVariance;
            double skill = Math.Max(0.0, Math.Min(1.0, profile.Skill + wobble));

            // bestFirst[0] is the top word; skill 1 → index 0, skill 0 → the weakest word.
            int index = (int)Math.Round((1.0 - skill) * (bestFirst.Count - 1));
            return bestFirst[index];
        }

        private float ChooseSubmitTime(float timeLimit)
        {
            float min = Math.Max(0f, Math.Min(profile.MinThinkSeconds, profile.MaxThinkSeconds));
            float max = Math.Max(profile.MinThinkSeconds, profile.MaxThinkSeconds);
            float time = min + (float)random.NextDouble() * (max - min);
            return Math.Min(time, timeLimit * LatestSubmitFraction);
        }
    }
}
