namespace XPSystem.EventHandlers
{
    using System;
    using MEC;
    using XPSystem.API;
    using XPSystem.API.StorageProviders;
    using static XPSystem.API.XPAPI;

    public class UnifiedEventHandlers
    {
        /// <summary>
        /// Gets invoke when a player without DNT joins the server.
        /// </summary>
        public static event Action<XPPlayer, PlayerInfoWrapper> PlayerJoined = delegate { }; 

        public virtual void RegisterEvents(Main plugin) {}
        public virtual void UnregisterEvents(Main plugin) {}

        protected void OnPlayerJoined(XPPlayer player)
        {
            if (XPAPI.StorageProvider != null)
            {
                if (player.DNT)
                {
                    XPAPI.StorageProvider.DeletePlayerInfo(player.PlayerId);
                    return;
                }

                Timing.CallDelayed(.5f, () =>
                {
                    var playerInfo = player.GetPlayerInfo();
#if STORENICKS
                    UpdateNickname(player);
#endif
                    PlayerJoined.Invoke(player, playerInfo);
                    DisplayProviders.Refresh(player);
                });
            }
        }
    }
}