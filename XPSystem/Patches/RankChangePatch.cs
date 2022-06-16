using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using XPSystem.API;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem.Patches
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetText))]
    public class RankChangePatch
    {
        internal static void Postfix(ServerRoles __instance, string i)
        {
            var ply = Player.Get(__instance._hub);
            var log = ply.GetLog();
            Badge badge = Main.Instance.Config.DNTBadge;
            if(!ply.DoNotTrack)
            {
                foreach (var kvp in Main.Instance.Config.LevelsBadge.OrderBy(kvp => kvp.Key))
                {
                    if (log.LVL > kvp.Key && Main.Instance.Config.LevelsBadge.OrderByDescending(kvp2 => kvp2.Key).ElementAt(0).Key != kvp.Key)
                        continue;
                    badge = kvp.Value;
                    break;
                }
            }
            string rankName = Main.Instance.Config.BadgeStructure
                .Replace("%lvl%", log.LVL.ToString())
                .Replace("%badge%", badge.Name)
                .Replace("%oldbadge%", ply.Group?.BadgeText);

            ply.RankColor = Main.Instance.Config.OverrideColor && ply.Group?.BadgeColor != null ?
                ply.Group?.BadgeColor :
                badge.Color;
            
            if (NetworkServer.active)
                __instance.Network_myText = rankName;
            __instance.MyText = rankName;
        }
    }
}
