namespace XPSystem.Patches
{
    using Exiled.API.Features;
    using HarmonyLib;
    using MEC;
    using API = API.API;

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetText))]
    public class RankChangePatch
    {
        internal static void Prefix(ServerRoles __instance, string i)
        {
            if (i != null && !i.Contains("\n"))
            {
                Timing.CallDelayed(1f, () =>
                {
                    API.UpdateBadge(Player.Get(__instance._hub), i);
                });
            }
        }
    }
}
