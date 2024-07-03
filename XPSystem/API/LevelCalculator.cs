namespace XPSystem.API
{
    using System.Collections.Generic;
    using System.Linq;
    using XPSystem.API.StorageProviders.Models;
    using static XPAPI;

    public static class LevelCalculator
    {
        private static readonly Dictionary<uint, (int xpPerLevel, int neededXP)> _xpNeededForLevel = new();
        private static int _firstIncreaseXP = 0;

        /// <summary>
        /// Gets the XP needed for the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The XP needed.</returns>
        public static int GetXP(int level)
        {
            if (level <= 0)
                return 0;

            int xp = 0;
            int xpPerLevel = (int)Config.XPPerLevel;

            if (_xpNeededForLevel.Any())
            {
                foreach (var kvp in _xpNeededForLevel)
                {
                    if (kvp.Key > level)
                        break;

                    xp += kvp.Value.neededXP;
                    xpPerLevel = kvp.Value.xpPerLevel;
                }
            }

            return xp + (level * xpPerLevel);
        }

        /// <summary>
        /// Gets the level of the specified <see cref="PlayerInfo"/>.
        /// </summary>
        /// <param name="playerInfo">The player info.</param>
        /// <returns>The level reached.</returns>
        public static int GetLevel(PlayerInfo playerInfo)
        {
            return GetLevel(playerInfo.XP, out _);
        }

        /// <summary>
        /// Gets the level reached with the specified amount of XP.
        /// </summary>
        /// <param name="xp">The amount of XP.</param>
        /// <param name="xpNeededCurrent">The amount needed for the current level.</param>
        /// <returns>The level reached.</returns>
        public static int GetLevel(int xp, out int xpNeededCurrent)
        {
            xpNeededCurrent = 0;
            if (xp < 0)
                return 0;

            if (Config.XPPerLevel == 0)
                return int.MaxValue;

            int level = 0;
            int totalNeeded = 0;
            int xpPerLevel = (int)Config.XPPerLevel;

            if (_xpNeededForLevel.Any() && xp > _firstIncreaseXP)
            {
                foreach (var kvp in _xpNeededForLevel)
                {
                    if (kvp.Value.neededXP > xp)
                        break;

                    xp -= kvp.Value.neededXP;

                    level = (int)kvp.Key;
                    xpPerLevel = kvp.Value.xpPerLevel;
                    totalNeeded = kvp.Value.neededXP;
                }
            }

            int over = (xp / xpPerLevel);
            xpNeededCurrent = totalNeeded + (over * xpPerLevel);
            return level + over;
        }

        /// <summary>
        /// Does some precalculation.
        /// Called on config change.
        /// </summary>
        public static void Precalculate()
        {
            _xpNeededForLevel.Clear();

            bool first = true;
            int alreadyRequiredXP = 0;
            int previousLevelStart = 0;
            int previousXPPerLevel = (int)Config.XPPerLevel;

            foreach (var kvp in Config.XPPerLevelExtra.OrderBy(x => x.Key))
            {
                int key = (int)kvp.Key;
                int xpPerLevel = (int)(Config.XPPerLevel + kvp.Value);
                alreadyRequiredXP += previousXPPerLevel * (key - previousLevelStart);

                if (first)
                {
                    _firstIncreaseXP = alreadyRequiredXP;
                    first = false;
                }

                _xpNeededForLevel[kvp.Key] = (xpPerLevel, alreadyRequiredXP);

                previousLevelStart = key;
                previousXPPerLevel = xpPerLevel;
            }
        }
    }
}