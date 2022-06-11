using System;
using Exiled.API.Features;
using HarmonyLib;
using XPSystem.API;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem.Patches
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetText))]
    public class RankChangePatch
    {
        internal static string Postfix(ServerRoles __instance)
        {
            var ply = Player.Get(__instance._hub);
            var comp = ply.GetXPComponent();
            Badge badge = Main.Instance.Config.DNTBadge;
            if(!ply.DoNotTrack)
            {
                foreach (var kvp in Main.Instance.Config.LevelsBadge)
                {
                    if (comp.log.LVL > kvp.Key)
                        continue;
                    badge = kvp.Value;
                }
            }
            string toReturn = Main.Instance.Config.BadgeStructure
                .Replace("%lvl%", comp.log.LVL.ToString())
                .Replace("%badge%", badge.Name)
                .Replace("%oldbadge%", ply.Group?.BadgeText);
            
            ply.RankColor = Main.Instance.Config.OverrideColor && ply.Group?.BadgeColor != null ?
                ply.Group?.BadgeColor :
                badge.Color;

            return toReturn;
        }
    }
}
