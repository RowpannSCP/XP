namespace XPSystem.EventHandlers
{
    using System;
    using System.Linq;
    using MEC;
    using XPSystem.API;
    using XPSystem.API.StorageProviders;
    using XPSystem.Config.Events;
    using static XPSystem.API.XPAPI;

    public class UnifiedEventHandlers
    {
        /// <summary>
        /// Gets invoke when a player without DNT joins the server.
        /// </summary>
        public static event Action<XPPlayer, PlayerInfoWrapper> XPPlayerJoined = delegate { }; 

        public virtual void RegisterEvents(Main plugin) {}
        public virtual void UnregisterEvents(Main plugin) {}

        protected void OnPlayerJoined(XPPlayer player)
        {
            if (XPAPI.StorageProvider != null)
            {
                if (player.DNT)
                {
                    player.DisplayMessage(Config.DNTMessage);
                    XPAPI.StorageProvider.DeletePlayerInfo(player.PlayerId);
                    return;
                }

                Timing.CallDelayed(.5f, () =>
                {
                    var playerInfo = player.GetPlayerInfo();
#if STORENICKS
                    UpdateNickname(player);
#endif
                    XPPlayerJoined.Invoke(player, playerInfo);
                    DisplayProviders.Refresh(player);
                });
            }
        }

        protected void OnRoundEnded()
        {
            if (Config.LogXPGainedMethods)
            {
                var list = XPECManager.KeyUsage
                    .OrderByDescending(x => x.Value)
                    .Take(25);

                LogInfo("25 most retrieved keys:");
                foreach (var kvp in list)
                {
                    LogInfo($"{kvp.Key}: {kvp.Value}");
                }

                XPECManager.KeyUsage.Clear();
            }
        }
    }
}