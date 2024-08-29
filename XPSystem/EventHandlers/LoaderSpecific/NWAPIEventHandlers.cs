namespace XPSystem.EventHandlers.LoaderSpecific
{
    using InventorySystem.Items;
    using InventorySystem.Items.Pickups;
    using MEC;
    using PlayerRoles;
    using PlayerRoles.Ragdolls;
    using PlayerStatsSystem;
    using PluginAPI.Core;
    using PluginAPI.Core.Attributes;
    using PluginAPI.Enums;
    using PluginAPI.Events;
    using XPSystem.API;

#if !EXILED
    public class NWAPIEventHandlers : UnifiedEventHandlers
    {
        public override void RegisterEvents(Main plugin) =>
            EventManager.RegisterEvents(plugin, this);

        public override void UnregisterEvents(Main plugin) =>
            EventManager.UnregisterEvents(plugin, this);

        [PluginEvent(ServerEventType.PlayerJoined)] // playerid null immediately after joining
        private void PlayerJoined(Player player) => Timing.CallDelayed(.5f, () => OnPlayerJoined(player));

        [PluginEvent(ServerEventType.PlayerLeft)]
        private void PlayerLeaving(Player player) => OnPlayerLeft(player);

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        private void PlayedChangedRole(Player plr, PlayerRoleBase oldRole, RoleTypeId newRole, RoleChangeReason reason) =>
            OnPlayerChangedRole(plr, oldRole.RoleTypeId, newRole);

        [PluginEvent(ServerEventType.RoundEnd)]
        private void RoundEnded(RoundSummary.LeadingTeam leadingTeam) => OnRoundEnded(leadingTeam);

        [PluginEvent(ServerEventType.RoundRestart)]
        private void RoundRestarting() => OnRoundRestarting();

        [PluginEvent(ServerEventType.PlayerDying)]
        private void PlayerDied(Player player, Player attacker, DamageHandlerBase damageHandler)
        {
            if (IsNull(player) || IsNull(attacker))
                return;

            OnPlayedDied(attacker, player);
        }

        [PluginEvent(ServerEventType.Scp914PickupUpgraded)]
        private void PlayerUpgradedItem(Scp914PickupUpgradedEvent ev)
        {
            if (IsNull(ev.Item.PreviousOwner.Hub))
                return;

            OnPlayerUpgradedItem(ev.Item.PreviousOwner.Hub, ev.Item.Info.ItemId.GetCategory());
        }

        [PluginEvent(ServerEventType.PlayerSearchedPickup)]
        private void PlayerPickedUpItem(Player player, ItemPickupBase item) => OnPlayerPickedUpItem(player, item.Info.ItemId.GetCategory());

#warning - Move it over to DroppedItem when possible
        [PluginEvent(ServerEventType.PlayerDropItem)]
        private void OnPlayerDroppedItem(Player player, ItemBase item) => OnPlayerDroppedItem(player, item.Category);

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        private void OnPlayerUsedItem(Player player, ItemBase item) => OnPlayerUsedItem(player, item.ItemTypeId);

        [PluginEvent(ServerEventType.PlayerSpawn)]
        private void OnSpawn(Player player, RoleTypeId role)
        {
            if (string.IsNullOrWhiteSpace(player.UserId))
                return;

            OnPlayerSpawned(player);
        }

        [PluginEvent(ServerEventType.PlayerEscape)]
        private void OnPlayerEscaped(Player player, RoleTypeId role) => OnPlayerEscaped(player);

        [PluginEvent(ServerEventType.Scp049ResurrectBody)]
        public void OnScp049ResurrectBody(Player player, Player target, BasicRagdoll body) => OnPlayerResurrected(player);
    }
#endif
}