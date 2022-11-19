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
    using LiteNetLib4Mirror.Open.Nat;

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetText))]
    public class RankChangePatch
    {
        internal static bool Prefix(ServerRoles __instance, string i)
        {
            var ply = Player.Get(__instance._hub);
            var log = ply.GetLog();
            bool hasGroup = ply.Group == Main.Instance.handlers.UserGroup;
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

            string newValue;
            if (i == null || !i.ContainsIgnoreCase("\n"))
            {
                newValue = !hasGroup && !string.IsNullOrEmpty(Main.Instance.Config.BadgeStructureNoBadge)
                    ? Main.Instance.Config.BadgeStructure
                        .Replace("%lvl%", log.LVL.ToString())
                        .Replace("%badge%", badge.Name)
                        .Replace("%oldbadge%", string.IsNullOrWhiteSpace(i) ? ply.Group?.BadgeText : i)
                    : Main.Instance.Config.BadgeStructureNoBadge
                        .Replace("%lvl%", log.LVL.ToString())
                        .Replace("%badge%", badge.Name);
                newValue += "\n";
            }
            else
            {
                newValue = i;
            }

            bool returnEarly = false;
            if (hasGroup)
            {
                ply.RankColor = badge.Color;
                returnEarly = true;
            }
            else if (Main.Instance.Config.OverrideColor)
            {
                ply.RankColor = badge.Color;
                returnEarly = true;
            }
            
            if (NetworkServer.active)
                __instance.Network_myText = newValue;
            __instance.MyText = newValue;

            if (returnEarly)
                return false;
            
            ServerRoles.NamedColor namedColor = __instance.NamedColors.FirstOrDefault(row => row.Name == __instance.MyColor);
            if (namedColor == null)
                return false;
            __instance.CurrentColor = namedColor;
            
            return false;

        }
    }
}
