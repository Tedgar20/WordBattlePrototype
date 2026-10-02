using System;

namespace WordBattle.Core
{
    /// <summary>Tuning for a computer opponent. Names must be original (not Quarrel's characters).</summary>
    [Serializable]
    public sealed class AiProfile
    {
        public string Name = "Pip";

        /// <summary>0 = picks among its weakest words, 1 = always finds the best word.</summary>
        public float Skill = 0.4f;

        /// <summary>
        /// Random wobble added to Skill each battle so the AI isn't perfectly predictable.
        /// </summary>
        public float SkillVariance = 0.15f;

        public float MinThinkSeconds = 8f;
        public float MaxThinkSeconds = 22f;
    }
}
