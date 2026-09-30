using System;

namespace WordBattle.Core
{
    [Serializable]
    public sealed class MatchConfig
    {
        /// <summary>Battle wins needed to take the match. MVP best-of-3 = 2; production best-of-1 = 1.</summary>
        public int RoundsToWin = 2;

        public float BattleTimeSeconds = 30f;

        public int RackSize = 8;
    }
}
