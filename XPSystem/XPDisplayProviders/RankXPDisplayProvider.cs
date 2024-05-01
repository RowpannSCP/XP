namespace XPSystem.XPDisplayProviders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using HarmonyLib;
    using MEC;
    using XPSystem.API;
    using XPSystem.API.DisplayProviders;
    using XPSystem.API.StorageProviders;
    using XPSystem.Config.Models;
    using YamlDotNet.Serialization;

    public class RankXPDisplayProvider : SyncVarXPDisplayProvider<RankXPDisplayProvider.RankConfig, Badge>
    {
        protected override string VariableKey { get; } = "RankXPDisplayProvider_badge";

        protected override (Type typeName, string methodName, Func<XPPlayer, Badge, object> getFakeSyncVar, Func<XPPlayer, object> getResyncVar)[] SyncVars { get; } =
        {
            (typeof(ServerRoles), nameof(ServerRoles.Network_myText), (_, obj) => obj.Text, player => player.BadgeText),
            (typeof(ServerRoles), nameof(ServerRoles.Network_myColor), (_, obj) => obj.Color.ToString().ToLower(), player => player.BadgeColor)
        };

        protected override Badge CreateObject(XPPlayer player, PlayerInfoWrapper playerInfo)
        {
            if (player.DNT)
                return Config.DNTBadge;

            Badge badge = null;
            string format = !player.HasBadge || player.HasHiddenBadge
                ? Config.BadgeStructureNoBadge
                : Config.BadgeStructure;

            foreach (var kvp in Config.SortedBadges)
            {
                if (kvp.Key > playerInfo.Level)
                    break;

                badge = kvp.Value;
            }

            if (badge == null)
                return null;

            return new Badge()
            {
                Text = format
                    .Replace("%lvl%", playerInfo.Level.ToString())
                    .Replace("%badge%", badge.Text)
                    .Replace("%oldbadge%", player.BadgeText),
                Color = badge.Color
            };
        }

        protected override bool ShouldEdit(XPPlayer player)
        {
            if (Config.SkipGlobalBadges && player.HasGlobalBadge)
                return false;

            if (!Config.EditBadgeHiding && player.HasHiddenBadge)
                return false;

            return true;
        }

        protected override bool ShouldShowTo(XPPlayer player, XPPlayer target)
        {
            if (!Config.EditBadgeHiding)
                return true;

            if (player.HasHiddenBadge && !target.CanViewHiddenBadge)
                return false;

            return true;
        }

        [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.TryHideTag))]
        internal static class HiddenBadgePatch
        {
            public static void Prefix(ServerRoles __instance)
            {
                foreach (IXPDisplayProvider provider in XPAPI.DisplayProviders)
                {
                    if (provider is RankXPDisplayProvider rankProvider && rankProvider.Config.PatchHiddenBadgeSetter)
                    {
                        Timing.CallDelayed(.5f, () =>
                        {
                            rankProvider.RefreshOf(__instance._hub);
                        });

                        return;
                    }
                }
            }
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

            [Description("Whether or not to patch the hidden badge setter to automatically update the badge.")]
            public bool PatchHiddenBadgeSetter { get; set; } = true;

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