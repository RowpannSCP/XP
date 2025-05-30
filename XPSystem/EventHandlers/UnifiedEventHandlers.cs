namespace XPSystem.EventHandlers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using MEC;
    using PlayerRoles;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.Config.Events;
    using XPSystem.Config.Events.Types;
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
                MethodBase method = new StackTrace()
                    .GetFrame(1)
                    .GetMethod();

                LogDebug($"Object typeof {typeof(T).FullName} is null in {method.DeclaringType}::{method.Name}");
                return true;
            }

            return false;
        }

        // Event needs to exist to ensure players are added to list
        protected void OnPlayerJoined(BaseXPPlayer player)
        {
            if (XPAPI.StorageProvider != null)
            {
                if (player is XPPlayer xpPlayer)
                {
                    Timing.CallDelayed(.5f + Config.ExtraDelay, () =>
                    {
                        PlayerInfoWrapper playerInfo = xpPlayer.GetPlayerInfo();
                        XPPlayerJoined.Invoke(xpPlayer, playerInfo);
#if STORENICKS
                        UpdateNickname(xpPlayer);
#endif
                        DisplayProviders.RefreshTo(xpPlayer);
                        DisplayProviders.RefreshOf(xpPlayer, playerInfo);
                    });
                }
                else if (player.DNT)
                {
                    player.DisplayMessage(Config.DNTMessage);

                    if (!player.UserId.TryParseUserId(out IPlayerId? playerId))
                    {
                        LogError($"Failed to parse DNT user ID for player {player.Nickname} ({player.UserId}) in order to attempt deletion");
                        return;
                    }

                    XPAPI.StorageProvider.DeletePlayerInfo(playerId);
                }
            }
        }

        protected void OnPlayerLeft(XPPlayer? player)
        {
            if (player == null)
                return;
            PlayerLeft.Invoke(player);
        }

        protected void OnPlayerChangedRole(XPPlayer? player, RoleTypeId oldRole, RoleTypeId newRole)
        {
            if (player == null)
                return;
            PlayerChangedRole.Invoke(player, oldRole, newRole);
        }

        protected void OnRoundEnded(RoundSummary.LeadingTeam leadingTeam)
        {
            if (!Config.XPAfterRoundEnd)
                XPGainPaused = true;

            XPECItem? roundwin = XPECManager.GetItem("win");
            foreach (XPPlayer player in XPPlayer.XPPlayers)
            {
                if (player.LeadingTeam == leadingTeam)
                    player.TryAddXPAndDisplayMessage(roundwin);
            }

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

        protected void OnPlayedDied(XPPlayer? attacker, XPPlayer? target, RoleTypeId? targetRole = null)
        {
            if (target != null)
            {
                targetRole ??= target.Role;
                target.TryAddXPAndDisplayMessage("death", targetRole);
            }

            if (attacker != null && attacker != target)
                attacker.TryAddXPAndDisplayMessage("kill", targetRole);
        }

        protected void OnPlayerUpgradedItem(XPPlayer? player, ItemCategory item) => player?.TryAddXPAndDisplayMessage("upgrade", item);
        protected void OnPlayerPickedUpItem(XPPlayer? player, ItemCategory item) => player?.TryAddXPAndDisplayMessage("pickup", item);
        protected void OnPlayerDroppedItem(XPPlayer? player, ItemCategory item) => player?.TryAddXPAndDisplayMessage("drop", item);
        protected void OnPlayerUsedItem(XPPlayer? player, ItemType item) => player?.TryAddXPAndDisplayMessage("use", item);

        protected void OnPlayerSpawned(XPPlayer? player) => player?.TryAddXPAndDisplayMessage("spawn");
        protected void OnPlayerEscaped(XPPlayer? player) => player?.TryAddXPAndDisplayMessage("escape");
        protected void OnPlayerResurrected(XPPlayer? scp049) => scp049?.TryAddXPAndDisplayMessage("resurrect");
    }
}