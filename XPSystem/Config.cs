using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem
{
    using System.Linq;
    using PlayerRoles;
    using YamlDotNet.Serialization;

    public class Config 
#if EXILED
        : Exiled.API.Interfaces.IConfig
#endif
    {
        [Description("Enable plugin?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Show debug messages?")] 
        public bool Debug { get; set; } = false;

        [Description("Available modes: Hint, Broadcast, Console, None.")]
        public HintMode HintMode { get; set; } = HintMode.Hint;

        [Description("Hint shown to the players if they have DNT enabled.")]
        public string DNTHint { get; set; } = "We can't track your stats while you have DNT enabled in your game options!";
        
        [Description("Badge for players with DNT enabled.")]
        public Badge DNTBadge { get; set; } = new Badge
        {
            Name = "(DNT) anonymous man????",
            Color = "nickel"
        };

        [Description("(You may add your own entries) Role1: Role2: XP player with Role1 gets for killing a person with Role2. RoleTypeId.None is used as default for both killer and viction roletype")]
        public Dictionary<RoleTypeId, Dictionary<RoleTypeId, int>> KillXP { get; set; } = new Dictionary<RoleTypeId, Dictionary<RoleTypeId, int>>()
        {
            [RoleTypeId.None] = new Dictionary<RoleTypeId, int>()
            {
                [RoleTypeId.None] = 0,
            },
            [RoleTypeId.ClassD] = new Dictionary<RoleTypeId, int>()
            {
                [RoleTypeId.Scientist] = 200,
                [RoleTypeId.FacilityGuard] = 150,
                [RoleTypeId.NtfPrivate] = 200,
                [RoleTypeId.NtfSergeant] = 250,
                [RoleTypeId.NtfCaptain] = 300,
                [RoleTypeId.Scp049] = 500,
                [RoleTypeId.Scp0492] = 100,
                [RoleTypeId.Scp106] = 500,
                [RoleTypeId.Scp173] = 500,
                [RoleTypeId.Scp096] = 500,
                [RoleTypeId.Scp939] = 500,
            },
            [RoleTypeId.Scientist] = new Dictionary<RoleTypeId, int>()
            {
                [RoleTypeId.ClassD] = 50,
                [RoleTypeId.ChaosConscript] = 200,
                [RoleTypeId.ChaosRifleman] = 200,
                [RoleTypeId.ChaosRepressor] = 250,
                [RoleTypeId.ChaosMarauder] = 300,
                [RoleTypeId.Scp049] = 500,
                [RoleTypeId.Scp0492] = 100,
                [RoleTypeId.Scp106] = 500,
                [RoleTypeId.Scp173] = 500,
                [RoleTypeId.Scp096] = 500,
                [RoleTypeId.Scp939] = 500,
            }
        };

        [Description("How many XP should a player get if their team wins.")]
        public int TeamWinXP { get; set; } = 250;

        [Description("How many XP is required to advance a level.")]
        public int XPPerLevel { get; set; } = 1000;

        [Description("Required xp = xpperlevel + xpperlevelincrease for level * level.")]
        public Dictionary<int, int> XPPerLevelIncreases { get; set; } = new Dictionary<int, int>()
        {
            [0] = 0,
            [100] = 1000,
        };

        private Dictionary<int, int> _xpPerLevelIncreasesOrdered;
        public Dictionary<int, int> GetIncreasesOrdered()
        {
            if (_xpPerLevelIncreasesOrdered != null)
                return _xpPerLevelIncreasesOrdered;
            return _xpPerLevelIncreasesOrdered = new Dictionary<int, int>(XPPerLevelIncreases)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        [Description("Show a message if a player gets XP")]
        public bool ShowAddedXP { get; set; } = true;

        [Description("Show a message to the player if he advances a level.")]
        public bool ShowAddedLVL { get; set; } = true;

        [Description("What message to show if player advances a level. (if ShowAddedLVL = false, this is irrelevant)")]
        public string AddedLVLHint { get; set; } = "NEW LEVEL: <color=red>%level%</color>";

        [Description("(You may add your own entries) How many XP a player gets for escaping")]
        public Dictionary<RoleTypeId, int> EscapeXP { get; set; } = new Dictionary<RoleTypeId, int>()
        {
            [RoleTypeId.ClassD] = 500,
            [RoleTypeId.Scientist] = 300
        };

        [Description("(You may add your own entries) How much xp a player gets for upgrading a item category")]
        public Dictionary<ItemCategory, int> UpgradeXP { get; set; } = new Dictionary<ItemCategory, int>()
        {
            [ItemCategory.Ammo] = 10,
            [ItemCategory.Armor] = 10,
            [ItemCategory.Firearm] = 10,
            [ItemCategory.Grenade] = 10,
            [ItemCategory.Keycard] = 10,
            [ItemCategory.Medical] = 10,
            [ItemCategory.Radio] = 10,
            [ItemCategory.MicroHID] = 10,
            [ItemCategory.SCPItem] = 10
        };

        [Description("Whether or not the xp for upgrading can only be gotten once per round")]
        public bool UpgradeXPOneTime { get; set; } = true;

        [Description("Use a timer to limit xp gain from an item. Will work with one time configs.")]
        public bool UseTimer { get; set; } = true;
        [Description("Whether the timer should count item itself or item types")]
        public bool TimerUseItemType { get; set; } = true;
        [Description("How long the timer should be")]
        public float TimerDuration { get; set; } = 60f;

        [Description("(You may add your own entries) How much xp a player gets for picking up an item, itemtype none for default")]
        public Dictionary<ItemType, int> PickupXP { get; set; } = new Dictionary<ItemType, int>()
        {
            [ItemType.None] = 0,
            [ItemType.Adrenaline] = 10,
        };

        [Description("Whether or not the xp for picking up items can only be gotten once per round, per item.")]// Will be ignore if PickupXPOneTimeItem is true")]
        public bool PickupXPOneTime { get; set; } = true;

        [Description("(You may add your own entries) How much xp a player gets for spawning, roletypeid none for default")]
        public Dictionary<RoleTypeId, int> SpawnXP { get; set; } = new Dictionary<RoleTypeId, int>()
        {
            [RoleTypeId.None] = 0,
            [RoleTypeId.ClassD] = 10,
        };

        [Description("(You may add your own entries) How much xp a player gets for dropping something, itemtype none for default")]
        public Dictionary<ItemType, int> DropXP { get; set; } = new Dictionary<ItemType, int>()
        {
            [ItemType.None] = 0,
            [ItemType.Adrenaline] = 10,
        };
        [Description("Whether or not the xp for dropping items can only be gotten once per round, per item.")]
        public bool DropXPOneTime { get; set; } = true;
        
        [Description("(You may add your own entries) How much xp a player gets for using something, itemtype none for default")]
        public Dictionary<ItemType, int> UseXP { get; set; } = new Dictionary<ItemType, int>()
        {
            [ItemType.None] = 0,
            [ItemType.Adrenaline] = 10,
        };

        [Description("Whether or not the xp for using items can only be gotten once per round, per item.")]
        public bool UseXPOneTime { get; set; } = true;

        [Description("If true, the level for the badge is the starting level where you will recieve it. If false, the level for the badge is what you will recieve as long as your level is below it")]
        public bool BadgeKeyIsRequiredLevel { get; set; } = false;

        [Description("(You may add your own entries) Level threshold and a badge. %color%. if you get a TAG FAIL in your console, either change your color, or remove special characters like brackets.")]
        public Dictionary<int, Badge> LevelsBadge { get; set; } = new Dictionary<int, Badge>()
        {
            [0] = new Badge
            {
                Name = "Visitor",
                Color = "cyan"
            },
            [1] = new Badge
            {
                Name = "Junior",
                Color = "orange"
            },
            [5] = new Badge
            {
                Name = "Senior",
                Color = "yellow"
            },
            [10] = new Badge
            {
                Name = "Veteran",
                Color = "red"
            },
            [50] = new Badge
            {
                Name = "Nerd",
                Color = "lime"
            }
        };

        [Description("Whether or not to enable nick changes. This will change the players nickname to the specified structure.")]
        public bool EnableNickMods { get; set; } = true;
        [Description("The structure of the player nick. Variables: %lvl% - the level. %name% - the players nickname/name")]
        public string NickStructure { get; set; } = "LVL %lvl% | %name%";

        [Description("Whether or not to enable badges. This will change the players badge to the specified structure.")]
        public bool EnableBadges { get; set; } = true;
        [Description("The structure of the badge displayed in-game. Variables: %lvl% - the level. %badge% earned badge in specified in LevelsBadge. %oldbadge% - base-game badge, like ones specified in config-remoteadmin, or a global badge. can be null.")]
        public string BadgeStructure { get; set; } = "%badge% | %oldbadge%";
        [Description("See above, just for people who dont have a badge. If empty, badgestructure will be used instead.")]
        public string BadgeStructureNoBadge { get; set; } = "%badge%";

        [Description("Override colors for people who already have a rank")]
        public bool OverrideColor { get; set; } = false;

        [Description("Size of the message.")]
        public byte HintSize { get; set; } = 100;
        
        [Description("Spacing of the message in (horizontal offset)")]
        public short HintSpace { get; set; } = 0;
        
        [Description("Vertical offset of the message.")]
        public byte VOffest { get; set; } = 0;
        
        [Description("Duration of the message.")]
        public float HintDuration { get; set; } = 3;

        [Description("Path the database gets saved to. May require change depending on OS.")]
        public string SavePath { get; set; } = Path.Combine(ConfigPath, @"Players.db");

        [Description("Path the text file for translations get saved to. May require change depending on OS.")]
        public string SavePathTranslations { get; set; } = Path.Combine(ConfigPath, @"xp-translations.yml");
        [Description("If disabled, will hide the global badge if set that way om the config")]
        public bool VSRComplaint { get; set; } = true;
        [Description("Whether or not to enable the getxp (client console) command")]
        public bool EnableGetXPCommand { get; set; } = true;
        [Description("Whether or not to change how badge hiding works")]
        public bool EditBadgeHiding { get; set; } = true;
        [Description("Whether or not to use different badges depending whether or not the player has view hidden badges permission")]
        public bool ShowHiddenBadgesToAdmins { get; set; } = true;

        [Description("Will print used keys to console at end of round.")]
        public bool LogXPGainedMethods { get; set; } = false;

        [YamlIgnore]
        private static string ConfigPath =>
#if !EXILED
        Path.Combine(PluginAPI.Helpers.Paths.LocalPlugins.Plugins, "XPSystem");
#else
        Exiled.API.Features.Paths.Configs;

        [Description("Disable to use single hints at a time, wont break other plugins.")]
        public bool EnableCustomHintManager { get; set; } = true;

        [Description("Increase for slower hint updates.")]
        public double HintDelay { get; set; } = 0.1f;

        [Description("Increase might help reduce flickering.")]
        public double HintExtraTime { get; set; } = 0.01f;

        [Description("(You may add your own entries) How much xp a player gets for interacting with a door")]
        public Dictionary<Exiled.API.Enums.DoorType, int> DoorInteractXP { get; set; } = new Dictionary<Exiled.API.Enums.DoorType, int>()
        {
            [Exiled.API.Enums.DoorType.Intercom] = 10,
        };

        [Description("Whether or not the xp for using doors can only be gotten once per round")]
        public bool DoorXPOneTime { get; set; } = true;

        [Description("Only useful if youre using remotekeycard")] 
        public bool DontGiveDoorXPEmptyItem { get; set; } = true;

        [Description("(You may add your own entries) How much xp a player gets for throwing something")]
        public Dictionary<Exiled.API.Enums.ProjectileType, int> ThrowXP { get; set; } = new Dictionary<Exiled.API.Enums.ProjectileType, int>()
        {
            [Exiled.API.Enums.ProjectileType.FragGrenade] = 10,
        };

        [Description("Displayer location of hints.")] 
        public AdvancedHints.Enums.DisplayLocation HintLocation { get; set; } = AdvancedHints.Enums.DisplayLocation.Top;
#endif
    }
}
