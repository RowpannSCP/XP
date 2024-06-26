namespace XPSystem.EventHandlers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using MEC;
    using PlayerRoles;
    using XPSystem.API;
    using XPSystem.API.StorageProviders;
    using XPSystem.Config.Events;
    using static XPSystem.API.XPAPI;

    public class UnifiedEventHandlers
    {
        /// <summary>
        /// Gets invoked when a player without DNT joins the server.
        /// </summary>
        public static event Action<XPPlayer, PlayerInfoWrapper> XPPlayerJoined = delegate { }; 

        /// <summary>
        /// Gets invoked when a player leaves the server.
        /// </summary>
        public static event Action<XPPlayer> PlayerLeft = delegate { }; 

        /// <summary>
        /// Gets invoked when a player changes their role.
        /// </summary>
        public static event Action<XPPlayer, RoleTypeId, RoleTypeId> PlayerChangedRole = delegate { }; 

        public virtual void RegisterEvents(Main plugin) {}
        public virtual void UnregisterEvents(Main plugin) {}

        protected bool IsNull<T>(T obj)
        {
            if (obj == null)
            {
                var method = new StackTrace()
                    .GetFrame(1)
                    .GetMethod();

                LogDebug($"Object typeof {typeof(T).FullName} is null in {method.DeclaringType}::{method.Name}");
                return true;
            }

            return false;
        }

        // Event needs to exist to ensure players are added to list
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
                    XPPlayerJoined.Invoke(player, playerInfo);
#if STORENICKS
                    UpdateNickname(player);
#endif
                    DisplayProviders.RefreshTo(player);
                    DisplayProviders.RefreshOf(player, playerInfo);
                });
            }
        }

        protected void OnPlayerLeft(XPPlayer player)
        {
            PlayerLeft.Invoke(player);
        }

        protected void OnPlayerChangedRole(XPPlayer player, RoleTypeId oldRole, RoleTypeId newRole)
        {
            PlayerChangedRole.Invoke(player, oldRole, newRole);
        }

        protected void OnRoundEnded(RoundSummary.LeadingTeam leadingTeam)
        {
            var roundwin = XPECManager.GetItem("win");
            foreach (var kvp in XPPlayer.Players)
                kvp.Value.AddXPAndDisplayMessage(roundwin);

            if (Config.LogXPGainedMethods)
            {
                var list = XPECManager.KeyUsage
                    .OrderByDescending(x => x.Value)
                    .Take(25);

                LogInfo("25 most retrieved keys:");
                foreach (var kvp in list)
                    LogInfo($"{kvp.Key}: {kvp.Value}");

                XPECManager.KeyUsage.Clear();
            }
        }

        protected void OnRoundRestarting()
        {
            XPPlayer.PlayersValue.Clear();
        }

        protected void OnPlayedDied(XPPlayer attacker, RoleTypeId targetRole) => attacker.TryAddXPAndDisplayMessage("kill", targetRole);
        protected void OnPlayerUpgradedItem(XPPlayer player, ItemCategory item) => player.TryAddXPAndDisplayMessage("upgrade", item);
        protected void OnPlayerPickedUpItem(XPPlayer player, ItemCategory item) => player.TryAddXPAndDisplayMessage("pickup", item);
        protected void OnPlayerDroppedItem(XPPlayer player, ItemCategory item) => player.TryAddXPAndDisplayMessage("drop", item);
        protected void OnPlayerUsedItem(XPPlayer player, ItemType item) => player.TryAddXPAndDisplayMessage("use", item);

        protected void OnPlayerSpawned(XPPlayer player) => player.TryAddXPAndDisplayMessage("spawn");
        protected void OnPlayerEscaped(XPPlayer player) => player.TryAddXPAndDisplayMessage("escape");
    }
}