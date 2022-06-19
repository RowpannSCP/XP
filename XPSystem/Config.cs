using Exiled.API.Features;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem
{
    public class Config : IConfig
    {
        [Description("Enable plugin?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Show debug messages?")] 
        public bool ShowDebug { get; set; } = false;

        [Description("Hint shown to the players if they have DNT enabled.")]
        public string DNTHint { get; set; } = "We can't track your stats while you have DNT enabled in your game options!";
        
        [Description("Badge for players with DNT enabled.")]
        public Badge DNTBadge { get; set; } = new Badge
        {
            Name = "(DNT) anonymous man????",
            Color = "nickel"
        };

        [Description("(You may add your own entries) Role1: Role2: XP player with Role1 gets for killing a person with Role2 ")]
        public Dictionary<RoleType, Dictionary<RoleType, int>> KillXP { get; set; } = new Dictionary<RoleType, Dictionary<RoleType, int>>()
        {
            [RoleType.ClassD] = new Dictionary<RoleType, int>()
            {
                [RoleType.Scientist] = 200,
                [RoleType.FacilityGuard] = 150,
                [RoleType.NtfPrivate] = 200,
                [RoleType.NtfSergeant] = 250,
                [RoleType.NtfCaptain] = 300,
                [RoleType.Scp049] = 500,
                [RoleType.Scp0492] = 100,
                [RoleType.Scp106] = 500,
                [RoleType.Scp173] = 500,
                [RoleType.Scp096] = 500,
                [RoleType.Scp93953] = 500,
                [RoleType.Scp93989] = 500,
            },
            [RoleType.Scientist] = new Dictionary<RoleType, int>()
            {
                [RoleType.ClassD] = 50,
                [RoleType.ChaosConscript] = 200,
                [RoleType.ChaosRifleman] = 200,
                [RoleType.ChaosRepressor] = 250,
                [RoleType.ChaosMarauder] = 300,
                [RoleType.Scp049] = 500,
                [RoleType.Scp0492] = 100,
                [RoleType.Scp106] = 500,
                [RoleType.Scp173] = 500,
                [RoleType.Scp096] = 500,
                [RoleType.Scp93953] = 500,
                [RoleType.Scp93989] = 500,
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
        public Dictionary<RoleType, int> EscapeXP { get; set; } = new Dictionary<RoleType, int>()
        {
            [RoleType.ClassD] = 500,
            [RoleType.Scientist] = 300
        };

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
        
        [Description("Path files get saved to. Requires change on linux.")]
        public string SavePath { get; set; } = Path.Combine(Paths.Configs, @"Players.db");
        
        [Description("Override colors for people who already have a rank")]
        public bool OverrideColor { get; set; } = false;
    }
}
