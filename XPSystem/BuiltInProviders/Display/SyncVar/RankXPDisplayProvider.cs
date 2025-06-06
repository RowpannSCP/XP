﻿namespace XPSystem.XPDisplayProviders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using HarmonyLib;
    using MEC;
    using XPSystem.API;
    using XPSystem.API.DisplayProviders;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using YamlDotNet.Serialization;

    public class RankXPDisplayProvider : SyncVarXPDisplayProvider<RankXPDisplayProvider.RankConfig, Badge>
    {
        protected override string VariableKey { get; } = "RankXPDisplayProvider_badge";

        protected override (Type typeName, string methodName, Func<BaseXPPlayer, Badge, object?> getFakeSyncVar, Func<BaseXPPlayer, object?> getResyncVar)[] SyncVars { get; } =
        {
            (typeof(ServerRoles), nameof(ServerRoles.Network_myText), (_, obj) => obj.Text, player => player.BadgeText),
            (typeof(ServerRoles), nameof(ServerRoles.Network_myColor), (_, obj) => obj.Color, player => player.BadgeColor)
        };

        protected override Badge? CreateObject(BaseXPPlayer player, PlayerInfoWrapper? playerInfo)
        {
            if (player.DNT)
            {
                if (player is { HasBadge: true, HasHiddenBadge: false })
                    return null;

                return Config.DNTBadge;
            }

            if (player is not XPPlayer xpPlayer)
                return null;
            playerInfo ??= xpPlayer.GetPlayerInfo();

            Badge? badge = null;
            string format = !player.HasBadge || player.HasHiddenBadge
                ? Config.BadgeStructureNoBadge
                : Config.BadgeStructure;
            string? color = null;

            foreach (var kvp in Config.SortedBadges)
            {
                if (kvp.Key > playerInfo.Level)
                    break;

                badge = kvp.Value;
                color = kvp.Value.Color;
            }

            if (badge == null)
                return null;

            if (player.HasBadge && !Config.OverrideColor)
                color = player.BadgeColor;

            return new Badge()
            {
                Text = format
                    .Replace("%lvl%", playerInfo.Level.ToString())
                    .Replace("%badge%", badge.Text)
                    .Replace("%oldbadge%", player.BadgeText),
                Color = color ?? "default"
            };
        }

        protected override bool ShouldEdit(BaseXPPlayer player)
        {
            if (Config.SkipGlobalBadges && player.HasGlobalBadge)
                return false;

            if (!Config.EditBadgeHiding && player.HasHiddenBadge)
                return false;

            return true;
        }

        protected override bool ShouldShowTo(BaseXPPlayer player, BaseXPPlayer target)
        {
            if (!base.ShouldShowTo(player, target))
                return false;
            if (!Config.EditBadgeHiding)
                return true;

            return !(player.HasHiddenBadge && (player == target || target.CanViewHiddenBadge));
        }

        public override void Enable()
        {
            base.Enable();

            Config.DNTBadge.ValidateColor();
            foreach (var kvp in Config.Badges)
                kvp.Value.ValidateColor();
        }

        internal static class HiddenBadgePatch
        {
            [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.RefreshLocalTag))]
            public class HiddenBadgePatchShow
            {
                public static void Postfix(ServerRoles __instance) => Refresh(__instance);
            }

            [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.TryHideTag))]
            public class HiddenBadgePatchHide
            {
                public static void Postfix(ServerRoles __instance) => Refresh(__instance);
            }

            private static void Refresh(ServerRoles instance)
            {
                foreach (IXPDisplayProvider provider in XPAPI.DisplayProviders)
                {
                    if (provider is RankXPDisplayProvider rankProvider && rankProvider.Config.Enabled && rankProvider.Config.PatchBadgeCommands)
                    {
                        Timing.CallDelayed(.5f + XPAPI.Config.ExtraDelay, () =>
                        {
                            rankProvider.RefreshOf(instance._hub);
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

            [Description("Override color if the user already has a badge?")]
            public bool OverrideColor { get; set; } = false;

            [Description("Required for VSR compliance.")]
            public bool SkipGlobalBadges { get; set; } = true;

            [Description("Whether or not to change how badge hiding works")]
            public bool EditBadgeHiding { get; set; } = true;

            [Description("Whether or not to patch the show & hide badge commands to automatically update the badge.")]
            public bool PatchBadgeCommands { get; set; } = true;

            [Description("Badge for players with dnt.")]
            public Badge DNTBadge { get; set; } = new()
            {
                Text = "(DNT) anonymous man????",
                Color = nameof(Misc.PlayerInfoColorTypes.Nickel)
            };

            private Dictionary<int, Badge> _sortedBadges = null!;
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
                    Color = nameof(Misc.PlayerInfoColorTypes.Cyan).ToLower()
                },
                [1] = new Badge
                {
                    Text = "Junior",
                    Color = nameof(Misc.PlayerInfoColorTypes.Orange).ToLower()
                },
                [5] = new Badge
                {
                    Text = "Senior",
                    Color = nameof(Misc.PlayerInfoColorTypes.Yellow).ToLower()
                },
                [10] = new Badge
                {
                    Text = "Veteran",
                    Color = nameof(Misc.PlayerInfoColorTypes.Red).ToLower()
                },
                [50] = new Badge
                {
                    Text = "Nerd",
                    Color = nameof(Misc.PlayerInfoColorTypes.Lime).ToLower()
                }
            };
        }
    }
}