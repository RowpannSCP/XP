namespace XPSystem.API
{
    using System;
    using NCalc;
    using XPSystem.API.StorageProviders.Models;
    using static XPAPI;

    public static class LevelCalculator
    {
        /// <summary>
        /// Gets the <see cref="Expression"/> used to calculate the level.
        /// </summary>
        public static Expression Expression { get; private set; }

        /// <summary>
        /// Gets the inverse of the <see cref="Expression"/>. Used to calculate the XP needed for a level.
        /// </summary>
        public static Expression InverseExpression { get; private set; }

        /// <summary>
        /// Gets the level of the specified <see cref="PlayerInfo"/>.
        /// </summary>
        /// <param name="playerInfo">The player info.</param>
        /// <returns>The level reached.</returns>
        public static int GetLevel(PlayerInfo playerInfo) => GetLevel(playerInfo.XP);

        /// <summary>
        /// Gets the level reached with the specified amount of XP.
        /// </summary>
        /// <param name="xp">The amount of XP.</param>
        /// <returns>The level reached.</returns>
        public static int GetLevel(int xp)
        {
            try
            {
                Expression.Parameters["xp"] = xp;
                return Convert.ToInt32(Expression.Evaluate() ?? 0);
            }
            catch (Exception e)
            {
                LogError($"Error calculating level: {e}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the XP needed for the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The XP needed.</returns>
        public static int GetXP(int level)
        {
            try
            {
                InverseExpression.Parameters["level"] = level;
                return Convert.ToInt32(InverseExpression.Evaluate() ?? 0);
            }
            catch (Exception e)
            {
                LogError($"Error calculating XP: {e}");
                return 0;
            }
        }

        /// <summary>
        /// Initializes the <see cref="Expression"/>.
        /// </summary>
        public static void Init()
        {
            try
            {
                Expression = new Expression(Config.LevelFunction);
            }
            catch (Exception e)
            {
                LogError($"Error initializing level function: {e}");
            }

            try
            {
                InverseExpression = new Expression(Config.XPFunction);
            }
            catch (Exception e)
            {
                LogError($"Error initializing inverse level function: {e}");
            }

            foreach (var kvp in Config.AdditionalFunctionParameters)
            {
                Expression.Parameters[kvp.Key] = kvp.Value;
                InverseExpression.Parameters[kvp.Key] = kvp.Value;
            }
        }
    }
}