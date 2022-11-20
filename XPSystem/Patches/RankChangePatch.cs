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
    using MEC;
    using API = API.API;

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetText))]
    public class RankChangePatch
    {
        internal static bool Prefix(ServerRoles __instance, string i)
        {
            if (!i.Contains("\n"))
            {
                Timing.CallDelayed(1f, () =>
                {
                    API.UpdateBadge(Player.Get(__instance._hub), i);
                });
            }
            if (i == string.Empty)
            {
                i = null;
            }
            if (NetworkServer.active)
            {
                __instance.Network_myText = i;
            }
            __instance.MyText = i;
            global::ServerRoles.NamedColor namedColor = __instance.NamedColors.FirstOrDefault(row => row.Name == __instance.MyColor);
            if (namedColor == null)
            {
                return false;
            }
            __instance.CurrentColor = namedColor;

            return false;
        }
    }
}
