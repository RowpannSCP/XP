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
        internal static bool Prefix(string i, ServerRoles __instance)
        {
            // actual method
            if (i == string.Empty)
                i = (string) null;
            if (NetworkServer.active)
                __instance.Network_myText = i;
            __instance.MyText = i;
            ServerRoles.NamedColor namedColor = ((IEnumerable<ServerRoles.NamedColor>) __instance.NamedColors).FirstOrDefault<ServerRoles.NamedColor>((Func<ServerRoles.NamedColor, bool>) (row => row.Name == __instance.MyColor));
            if (namedColor == null)
                return false;
            __instance.CurrentColor = namedColor;

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

            return false;
        }
    }
}
