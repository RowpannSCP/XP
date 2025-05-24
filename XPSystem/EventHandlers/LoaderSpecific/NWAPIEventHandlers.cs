namespace XPSystem.EventHandlers.LoaderSpecific
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp049Events;
    using LabApi.Events.Arguments.Scp914Events;
    using LabApi.Events.Arguments.ServerEvents;
    using XPSystem.API;

#if !EXILED
    public class NWAPIEventHandlers : UnifiedEventHandlers
    {
        public override void RegisterEvents(Main plugin)
        {
            LabApi.Events.Handlers.PlayerEvents.Joined += PlayerJoined;
            LabApi.Events.Handlers.PlayerEvents.Left += PlayerLeaving;
            LabApi.Events.Handlers.PlayerEvents.ChangedRole += PlayedChangedRole;
            LabApi.Events.Handlers.ServerEvents.RoundEnded += RoundEnded;
            LabApi.Events.Handlers.ServerEvents.RoundRestarted += RoundRestarting;

            LabApi.Events.Handlers.PlayerEvents.Dying += PlayerDied;
            LabApi.Events.Handlers.Scp914Events.ProcessedPickup += PlayerUpgradedPickup;
            LabApi.Events.Handlers.Scp914Events.ProcessedInventoryItem += PlayerUpgradedItem;
            LabApi.Events.Handlers.PlayerEvents.PickedUpItem += PlayerPickedUpItem;
            LabApi.Events.Handlers.PlayerEvents.DroppedItem += PlayerDroppedItem;
            LabApi.Events.Handlers.PlayerEvents.UsedItem += PlayerUsedItem;
            LabApi.Events.Handlers.PlayerEvents.Spawned += PlayerSpawned;
            LabApi.Events.Handlers.PlayerEvents.Escaped += PlayerEscaping;

            LabApi.Events.Handlers.Scp049Events.ResurrectedBody += Scp049FinishingRecall;
        }

        public override void UnregisterEvents(Main plugin)
        {
            LabApi.Events.Handlers.PlayerEvents.Joined -= PlayerJoined;
            LabApi.Events.Handlers.PlayerEvents.Left -= PlayerLeaving;
            LabApi.Events.Handlers.PlayerEvents.ChangedRole -= PlayedChangedRole;
            LabApi.Events.Handlers.ServerEvents.RoundEnded -= RoundEnded;
            LabApi.Events.Handlers.ServerEvents.RoundRestarted -= RoundRestarting;

            LabApi.Events.Handlers.PlayerEvents.Dying -= PlayerDied;
            LabApi.Events.Handlers.Scp914Events.ProcessedPickup -= PlayerUpgradedPickup;
            LabApi.Events.Handlers.Scp914Events.ProcessedInventoryItem -= PlayerUpgradedItem;
            LabApi.Events.Handlers.PlayerEvents.PickedUpItem -= PlayerPickedUpItem;
            LabApi.Events.Handlers.PlayerEvents.DroppedItem -= PlayerDroppedItem;
            LabApi.Events.Handlers.PlayerEvents.UsedItem -= PlayerUsedItem;
            LabApi.Events.Handlers.PlayerEvents.Spawned -= PlayerSpawned;
            LabApi.Events.Handlers.PlayerEvents.Escaped -= PlayerEscaping;

            LabApi.Events.Handlers.Scp049Events.ResurrectedBody -= Scp049FinishingRecall;
        }

        private void PlayerJoined(PlayerJoinedEventArgs ev) => OnPlayerJoined(ev.Player);
        private void PlayerLeaving(PlayerLeftEventArgs ev) => OnPlayerLeft(ev.Player);
        private void PlayedChangedRole(PlayerChangedRoleEventArgs ev) => OnPlayerChangedRole(ev.Player, ev.Player.Role, ev.NewRole.RoleTypeId);

        protected void RoundEnded(RoundEndedEventArgs ev) => OnRoundEnded(ev.LeadingTeam);
        private void RoundRestarting() => OnRoundRestarting();

        private void PlayerDied(PlayerDyingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (IsNull(ev.Player) || IsNull(ev.Attacker))
                return;

            OnPlayedDied(ev.Attacker, ev.Player, ev.Player.Role);
        }

        private void PlayerUpgradedPickup(Scp914ProcessedPickupEventArgs ev)
        {
            if (IsNull(ev.Pickup?.LastOwner))
                return;

            OnPlayerUpgradedItem(ev.Pickup!.LastOwner, ev.Pickup.Type.GetCategory());
        }

        private void PlayerUpgradedItem(Scp914ProcessedInventoryItemEventArgs ev) => OnPlayerUpgradedItem(ev.Player, ev.Item.Category);
        private void PlayerPickedUpItem(PlayerPickedUpItemEventArgs ev) => OnPlayerPickedUpItem(ev.Player, ev.Item.Category);

        private void PlayerDroppedItem(PlayerDroppedItemEventArgs ev)
        {
            if (IsNull(ev.Player) || IsNull(ev.Pickup))
                return;

            OnPlayerDroppedItem(ev.Player, ev.Pickup.Type.GetCategory());
        }

        private void PlayerUsedItem(PlayerUsedItemEventArgs ev) => OnPlayerUsedItem(ev.Player, ev.UsableItem.Type);
        private void PlayerSpawned(PlayerSpawnedEventArgs ev)
        {
            if (!ev.Player.IsReady)
                return;
            OnPlayerSpawned(ev.Player);
        }

        private void PlayerEscaping(PlayerEscapedEventArgs ev) => OnPlayerEscaped(ev.Player);
        private void Scp049FinishingRecall(Scp049ResurrectedBodyEventArgs ev) => OnPlayerResurrected(ev.Player);
    }
#endif
}