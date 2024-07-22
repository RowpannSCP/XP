namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using Flee.PublicTypes;
    using XPSystem.API.StorageProviders.Models;
    using static XPAPI;

    public static class LevelCalculator
    {
        private static Dictionary<int, int> XPLevelOverrides { get; } = new();

        /// <summary>
        /// Gets the <see cref="ExpressionContext"/> used for the <see cref="Expression"/>s.
        /// </summary>
        public static ExpressionContext Context { get; private set; }

        /// <summary>
        /// Gets the <see cref="IGenericExpression{T}"/> used to calculate the level.
        /// </summary>
        public static IGenericExpression<double> Expression { get; private set; }

        /// <summary>
        /// Gets the inverse of the <see cref="Expression"/>. Used to calculate the XP needed for a level.
        /// </summary>
        public static IGenericExpression<double> InverseExpression { get; private set; }

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
        /// <param name="throw">Whether to throw an exception if an error occurs.</param>
        /// <returns>The level reached.</returns>
        public static int GetLevel(int xp, bool @throw = false)
        {
            if (XPLevelOverrides.TryGetValue(xp, out int level))
                return level;

            try
            {
                Context.Variables["xp"] = xp;
                return Convert.ToInt32(Expression.Evaluate());
            }
            catch (Exception e) when (!@throw)
            {
                LogError($"Error calculating level: {e}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the XP needed for the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="throw">Whether to throw an exception if an error occurs.</param>
        /// <returns>The XP needed.</returns>
        public static int GetXP(int level, bool @throw = false)
        {
            if (Config.LevelXPOverrides.TryGetValue(level, out int xp))
                return xp;

            try
            {
                Context.Variables["level"] = level;
                return Convert.ToInt32(InverseExpression.Evaluate());
            }
            catch (Exception e) when (!@throw)
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
            foreach (var kvp in Config.LevelXPOverrides)
                XPLevelOverrides[kvp.Value] = kvp.Key;

            Context = new ExpressionContext();
            Context.Options.IntegersAsDoubles = true;
            Context.Imports.AddType(typeof(Math));

            Context.Variables.Add("xp", 0);
            Context.Variables.Add("level", 0);

            foreach (var kvp in Config.AdditionalFunctionParameters)
                Context.Variables.Add(kvp.Key, kvp.Value);

            try
            {
                Expression = Context.CompileGeneric<double>(Config.LevelFunction);
            }
            catch (Exception e)
            {
                LogError($"Error initializing level function: {e}");
            }

            try
            {
                InverseExpression = Context.CompileGeneric<double>(Config.XPFunction);
            }
            catch (Exception e)
            {
                LogError($"Error initializing inverse level function: {e}");
            }
        }
    }
}