namespace XPSystem.EventHandlers
{
    using System;
    using XPSystem.API;
    using XPSystem.API.StorageProviders.Models;
    using static XPSystem.API.XPAPI;

    public class UnifiedEventHandlers
    {
        /// <summary>
        /// Gets invoke when a player without DNT joins the server.
        /// </summary>
        public static event Action<XPPlayer, PlayerInfo> PlayerJoined = delegate { }; 
        public virtual void RegisterEvents(Main plugin) {}
        public virtual void UnregisterEvents(Main plugin) {}

        protected void OnPlayerJoined(XPPlayer player)
        {
            if (StorageProvider != null)
            {
                if (player.DNT)
                {
                    StorageProvider.DeletePlayerInfo(player.GetPlayerId());
                    return;
                }

                var playerInfo = StorageProvider.GetPlayerInfoAndCreateOfNotExist(player.GetPlayerId());
                PlayerJoined.Invoke(player, playerInfo);
                DisplayProviders.Refresh(player);
            }
        }
    }
}