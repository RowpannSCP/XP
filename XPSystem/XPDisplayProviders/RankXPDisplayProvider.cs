namespace XPSystem.XPDisplayProviders
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using XPSystem.API;
    using XPSystem.API.StorageProviders;
    using XPSystem.Config.Models;
    using YamlDotNet.Serialization;

    public class RankXPDisplayProvider : XPDisplayProvider<RankXPDisplayProvider.RankConfig>
    {
        public override void Enable() => RefreshAll();
        public override void Disable() => RefreshAll();

        public override void Refresh(XPPlayer player, PlayerInfoWrapper playerInfo)
        {
            if (!Config.Enabled)
            {
                player.ResyncSyncVar(typeof(ServerRoles), nameof(ServerRoles.Network_myText));
                player.ResyncSyncVar(typeof(ServerRoles), nameof(ServerRoles.Network_myColor));
                return;
            }

            if (Config.SkipGlobalBadges && player.HasGlobalBadge)
                return;

            if (player.HasBadge && (!player.HasHiddenBadge || Config.EditBadgeHiding))
                return;

            if (player.DNT)
            {
                player.SetBadge(Config.DNTBadge, true);
            }
            else
            {
                string text = GetBadgeText(player, playerInfo, out var color);
                if (text != null)
                {
                    player.SetBadge(new Badge
                    {
                        Text = text,
                        Color = color
                    }, true);
                }
            }
        }

        private string GetBadgeText(XPPlayer player, PlayerInfoWrapper playerInfo, out Misc.PlayerInfoColorTypes color)
        {
            string format = player.Group == null ? Config.BadgeStructureNoBadge : Config.BadgeStructure;
            Badge badge = null;
            color = Misc.PlayerInfoColorTypes.White;

            foreach (var kvp in Config.SortedBadges)
            {
                if (kvp.Key > playerInfo.Level)
                    break;

                badge = kvp.Value;
                color = badge.Color;
            }

            if (badge == null)
                return null;

            return format
                .Replace("%lvl%", player.GetPlayerInfo().Level.ToString())
                .Replace("%badge%", badge.Text)
                .Replace("%oldbadge%", player.Group?.BadgeText);
        }

        public class RankConfig : IXPDisplayProviderConfig
        {
            [Description("Enable badge modifications?")]
            public bool Enabled { get; set; } = true;

            [Description("The structure of the badge displayed in-game. Variables: %lvl% - the level. %badge% earned badge in specified in LevelsBadge. %oldbadge% - base-game badge, like ones specified in config-remoteadmin, or a global badge. can be null.")]
            public string BadgeStructure { get; set; } = "%badge% | %oldbadge%";

            [Description("See above, just for people who dont have a badge. If empty, badgestructure will be used instead.")]
            public string BadgeStructureNoBadge { get; set; } = "%badge%";

            [Description("Required for VSR compliance.")]
            public bool SkipGlobalBadges { get; set; } = true;

            [Description("Whether or not to change how badge hiding works")]
            public bool EditBadgeHiding { get; set; } = true;

            [Description("Badge for players with dnt.")]
            public Badge DNTBadge { get; set; } = new()
            {
                Text = "(DNT) anonymous man????",
                Color = Misc.PlayerInfoColorTypes.Nickel
            };

            private Dictionary<int, Badge> _sortedBadges;
            [YamlIgnore]
            public Dictionary<int, Badge> SortedBadges => _sortedBadges ??= Badges
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            [Description("(You may add your own entries) Level threshold and a badge. If you get a TAG FAIL in your console, either change your color, or remove special characters like brackets.")]
            public Dictionary<int, Badge> Badges { get; set; } = new()
            {
                [0] = new Badge
                {
                    Text = "Visitor",
                    Color = Misc.PlayerInfoColorTypes.Cyan
                },
                [1] = new Badge
                {
                    Text = "Junior",
                    Color = Misc.PlayerInfoColorTypes.Orange
                },
                [5] = new Badge
                {
                    Text = "Senior",
                    Color = Misc.PlayerInfoColorTypes.Yellow
                },
                [10] = new Badge
                {
                    Text = "Veteran",
                    Color = Misc.PlayerInfoColorTypes.Red
                },
                [50] = new Badge
                {
                    Text = "Nerd",
                    Color = Misc.PlayerInfoColorTypes.Lime
                }
            };
        }
    }
}