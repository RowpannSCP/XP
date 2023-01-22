using Exiled.API.Features;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem
{
    using AdvancedHints.Enums;
    using Exiled.API.Enums;
    using PlayerRoles;

    public class Config : IConfig
    {
        [Description("Enable plugin?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Show debug messages?")] 
        public bool Debug { get; set; } = false;

        [Description("Hint shown to the players if they have DNT enabled.")]
        public string DNTHint { get; set; } = "We can't track your stats while you have DNT enabled in your game options!";
        
        [Description("Badge for players with DNT enabled.")]
        public Badge DNTBadge { get; set; } = new Badge
        {
            Name = "(DNT) anonymous man????",
            Color = "nickel"
        };

        [Description("(You may add your own entries) Role1: Role2: XP player with Role1 gets for killing a person with Role2 ")]
        public Dictionary<RoleTypeId, Dictionary<RoleTypeId, int>> KillXP { get; set; } = new Dictionary<RoleTypeId, Dictionary<RoleTypeId, int>>()
        {
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

        [Description("Show a mini-hint if a player gets XP")]
        public bool ShowAddedXP { get; set; } = true;

        [Description("Show a hint to the player if he advances a level.")]
        public bool ShowAddedLVL { get; set; } = true;

        [Description("What hint to show if player advances a level. (if ShowAddedLVL = false, this is irrelevant)")]
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

        [Description("(You may add your own entries) How much xp a player gets for interacting with a door")]
        public Dictionary<DoorType, int> DoorInteractXP { get; set; } = new Dictionary<DoorType, int>()
        {
            [DoorType.Intercom] = 10,
        };
        
        [Description("Whether or not the xp for using doors can only be gotten once per round")]
        public bool DoorXPOneTime { get; set; } = true;

        [Description("Only useful if youre using remotekeycard")] 
        public bool DontGiveDoorXPEmptyItem { get; set; } = true;

        [Description("(You may add your own entries) How much xp a player gets for picking up an item")]
        public Dictionary<ItemType, int> PickupXP { get; set; } = new Dictionary<ItemType, int>()
        {
            [ItemType.Adrenaline] = 10,
        };

        [Description("Whether or not the xp for picking up items can only be gotten once per round, per item")]
        public bool PickupXPOneTimeItem { get; set; } = true;
        [Description("Whether or not the xp for picking up items can only be gotten once per round, per item. Will be ignore if PickupXPOneTimeItem is true")]
        public bool PickupXPOneTime { get; set; } = true;

        [Description("(You may add your own entries) How much xp a player gets for spawning")]
        public Dictionary<RoleTypeId, int> SpawnXP { get; set; } = new Dictionary<RoleTypeId, int>()
        {
            [RoleTypeId.ClassD] = 10,
        };
        
        [Description("(You may add your own entries) How much xp a player gets for throwing something")]
        public Dictionary<ProjectileType, int> ThrowXP { get; set; } = new Dictionary<ProjectileType, int>()
        {
            [ProjectileType.FragGrenade] = 10,
        };
        
        [Description("(You may add your own entries) How much xp a player gets for dropping something")]
        public Dictionary<ItemType, int> DropXP { get; set; } = new Dictionary<ItemType, int>()
        {
            [ItemType.Adrenaline] = 10,
        };
        [Description("Whether or not the xp for drop up items can only be gotten once per round, per item.")]
        public bool DropXPOneTime { get; set; } = true;

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
        
        [Description("The structure of the player nick. Variables: %lvl% - the level. %name% - the players nickname/name")]
        public string NickStructure { get; set; } = "LVL %lvl% | %name%";

        [Description("The structure of the badge displayed in-game. Variables: %lvl% - the level. %badge% earned badge in specified in LevelsBadge. %oldbadge% - base-game badge, like ones specified in config-remoteadmin, or a global badge. can be null.")]
        public string BadgeStructure { get; set; } = "%badge% | %oldbadge%";
        [Description("See above, just for people who dont have a badge. If empty, badgestructure will be used instead.")]
        public string BadgeStructureNoBadge { get; set; } = "%badge%";
        
        [Description("Path the database gets saved to. Requires change on linux.")]
        public string SavePath { get; set; } = Path.Combine(Paths.Configs, @"Players.db");

        [Description("Path the text file for translations get saved to. Requires change on linux.")]
        public string SavePathTranslations { get; set; } = Path.Combine(Paths.Configs, @"xp-translations.yml");
        
        [Description("Override colors for people who already have a rank")]
        public bool OverrideColor { get; set; } = false;

        [Description("Displayer location of hints.")] 
        public DisplayLocation HintLocation { get; set; } = DisplayLocation.Top;
        
        [Description("Size of hints.")]
        public byte HintSize { get; set; } = 100;
        
        [Description("Spacing of the in (horizontal offset)")]
        public short HintSpace { get; set; } = 0;
        
        [Description("Vertical offset of hints.")]
        public byte VOffest { get; set; } = 0;
        
        [Description("Duration of hints.")]
        public float HintDuration { get; set; } = 3;
    }
}
