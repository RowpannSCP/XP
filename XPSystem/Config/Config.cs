namespace XPSystem.Config
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using XPSystem.API.Enums;

    public abstract class Config
    {
        [Description("Print debug messages?")]
        public bool Debug { get; set; } = false;

        [Description("The function to calculate level for given xp. Parameter: xp. Available functions: https://learn.microsoft.com/en-us/dotnet/api/system.math?view=net-8.0#methods.")]
        public string LevelFunction { get; set; } = "Ceiling(-50 + Sqrt((4 * xp / a) + 9800) / 2)";

        [Description("The function to calculate xp needed for a level. The inverse of the LevelFunction. Parameter: level. Available functions: https://learn.microsoft.com/en-us/dotnet/api/system.math?view=net-8.0#methods.")]
        public string XPFunction { get; set; } = "Ceiling((level^2 + 100 * level + 50) * a)";

        [Description("Additional parameters for the level/xp functions.")]
        public Dictionary<string, double> AdditionalFunctionParameters { get; set; } = new()
        {
            { "a", 1 },
        };

        [Description("Override xp required for specific levels. Key: level, Value: xp.")]
        public Dictionary<int, int> LevelXPOverrides { get; set; } = new()
        {
            [0] = 0
        };

        [Description("Whether or not to pause xp gain when the round ends.")]
        public bool XPAfterRoundEnd { get; set; } = true;

        [Description("A global XP multiplier.")]
        public float GlobalXPMultiplier { get; set; } = 1f;

        [Description("Whether or not the global XP multiplier should apply to xp given to people that aren't online (via commands, etc.).")]
        public bool GlobalXPMultiplierForNonOnline { get; set; } = true;

        [Description("Whether or not the global XP multiplier should apply to xp removed.")]
        public bool XPMultiplierForXPLoss { get; set; } = false;

        [Description("Path to the event configs folder, relative to the ExtendedConfigPath.")]
        public string EventConfigsFolder { get; set; } = "XPEventConfigs";

        [Description("Whether or not to enable the .xp get (client) command")]
        public bool EnableGetXPCommand { get; set; } = true;

        [Description("Whether or not to enable the .xp leaderboard (client) command")]
        public bool EnableLeaderboardCommand { get; set; } = true;

        [Description("The maximum amount of entries that may be retrieved with the client leaderboard command.")]
        public byte LeaderboardMaxLength { get; set; } = 10;

        [Description("Additional update delay for XP Display providers. Increasing may help when it only works for some players.")]
        public float ExtraDelay { get; set; } = 0f;

        [Description("The message to show to players who have DNT enabled.")]
        public string DNTMessage { get; set; } =
            "We can't track your stats while you have DNT enabled in your game options!";

        [Description("Whether or not to format a message according to a template when adding xp.")]
        public bool UseAddedXPTemplate { get; set; } = true;

        [Description("When enabled, template used for messages that modify xp. Parameters: %message%, %currentxp%, %currentlevel%, %neededxp%, %nextlevel.")]
        public string AddedXPTemplate { get; set; } = "%message%, (%currentxp% / %neededxp%)";

        [Description("Whether or not to use the total xp instead of only the xp required for the next level. Requires extra calculations if false.")]
        public bool UseTotalXP { get; set; } = true;

        [Description("Whether or not to show a message to a player if they advance a level.")]
        public bool ShowAddedLVL { get; set; } = true;

        [Description("When enabled, what message to show if player advances a level.")]
        public string AddedLVLMessage { get; set; } = "NEW LEVEL: <color=red>%level%</color>";

        [Description("Decide how messages (ex. xp gain, level up) are displayed.")]
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Hint;

        [Description("The duration of the message, if applicable.")]
        public float DisplayDuration { get; set; } = 5f;

        [Description("Appended to all messages.")]
        public string TextPrefix { get; set; } = "";

        [Description("Prepended to all messages.")]
        public string TextSuffix { get; set; } = "";

        [Description("An alias for the client to view their own xp. Empty to disable.")]
        public string ClientGetCommandAlias { get; set; } = "";

        [Description("An alias for the client to view the leaderboard. Empty to disable.")]
        public string ClientLeaderboardCommandAlias { get; set; } = "";

        [Description("The assembly qualified type name of the storage provider to use (default: XPSystem.BuiltInProviders.LiteDB.LiteDBProvider) (not assembly qualified because it's built-in, unlike yours)." +
                     "Available, but I will not help you with: XPSystem.BuiltInProviders.MySql.MySqlProvider")]
        public string StorageProvider { get; set; } = "XPSystem.BuiltInProviders.LiteDB.LiteDBProvider";

        [Description("The assembly qualified type names of xp display providers to load, in addition to the built-in rank and nick ones.")]
        public List<string> AdditionalDisplayProviders { get; set; } = new()
        {
        };

        [Description("Prints used keys to console at end of round. Not recommended to keep on - performance.")]
        public bool LogXPGainedMethods { get; set; } = false;
        
        public abstract string ExtendedConfigPath { get; set; }
        public abstract string LegacyDefaultDatabasePath { get; set; }
    }
}