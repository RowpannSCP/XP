namespace XPSystem.EventHandlers.LoaderSpecific
{
    using System;
    using Exiled.Events.EventArgs.Interfaces;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Scp049;
    using Exiled.Events.EventArgs.Scp914;
    using Exiled.Events.EventArgs.Server;
    using XPSystem.API;
    using static API.XPAPI;

#if EXILED
    public class ExiledEventHandlers : UnifiedEventHandlers
    {
        public override void RegisterEvents(Main plugin)
        {
            Exiled.Events.Handlers.Player.Verified += PlayerJoined;
            Exiled.Events.Handlers.Player.Destroying += PlayerLeaving;
            Exiled.Events.Handlers.Player.ChangingRole += PlayedChangedRole;
            Exiled.Events.Handlers.Server.RoundEnded += RoundEnded;
            Exiled.Events.Handlers.Server.RestartingRound += RoundRestarting;

            Exiled.Events.Handlers.Player.Died += PlayerDied;
            Exiled.Events.Handlers.Scp914.UpgradingPickup += PlayerUpgradingPickup;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem += PlayerUpgradingItem;
            Exiled.Events.Handlers.Player.ItemAdded += PlayerAddedItem;
            Exiled.Events.Handlers.Player.DroppedItem += PlayerDroppedItem;
            Exiled.Events.Handlers.Player.UsedItem += PlayerUsedItem;
            Exiled.Events.Handlers.Player.Spawned += PlayerSpawned;
            Exiled.Events.Handlers.Player.Escaping += PlayerEscaping;

            Exiled.Events.Handlers.Scp049.FinishingRecall += Scp049FinishingRecall;

            Exiled.Events.Handlers.Player.ThrownProjectile += PlayerThrown;
            Exiled.Events.Handlers.Player.InteractingDoor += PlayerInteractingDoor;
            Exiled.Events.Handlers.Player.ActivatingGenerator += PlayerActivatingGenerator;
        }

        public override void UnregisterEvents(Main plugin)
        {
            Exiled.Events.Handlers.Player.Verified -= PlayerJoined;
            Exiled.Events.Handlers.Player.Destroying -= PlayerLeaving;
            Exiled.Events.Handlers.Player.ChangingRole -= PlayedChangedRole;
            Exiled.Events.Handlers.Server.RoundEnded -= RoundEnded;
            Exiled.Events.Handlers.Server.RestartingRound -= RoundRestarting;

            Exiled.Events.Handlers.Player.Died -= PlayerDied;
            Exiled.Events.Handlers.Scp914.UpgradingPickup -= PlayerUpgradingPickup;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem -= PlayerUpgradingItem;
            Exiled.Events.Handlers.Player.ItemAdded -= PlayerAddedItem;
            Exiled.Events.Handlers.Player.DroppedItem -= PlayerDroppedItem;
            Exiled.Events.Handlers.Player.UsedItem -= PlayerUsedItem;
            Exiled.Events.Handlers.Player.Spawned -= PlayerSpawned;
            Exiled.Events.Handlers.Player.Escaping -= PlayerEscaping;

            Exiled.Events.Handlers.Scp049.FinishingRecall -= Scp049FinishingRecall;

            Exiled.Events.Handlers.Player.ThrownProjectile -= PlayerThrown;
            Exiled.Events.Handlers.Player.InteractingDoor -= PlayerInteractingDoor;
            Exiled.Events.Handlers.Player.ActivatingGenerator -= PlayerActivatingGenerator;
        }

        private void PlayerJoined(VerifiedEventArgs ev) => OnPlayerJoined(ev.Player);
        private void PlayerLeaving(DestroyingEventArgs ev) => OnPlayerLeft(ev.Player);
        private void PlayedChangedRole(ChangingRoleEventArgs ev) => OnPlayerChangedRole(ev.Player, ev.Player.Role, ev.NewRole);

        protected void RoundEnded(RoundEndedEventArgs ev)
        {
            if (!Enum.TryParse(ev.LeadingTeam.ToString(), out RoundSummary.LeadingTeam leadingTeam))
            {
                LogWarn("Exiled LeadingTeam not basegame LeadingTeam! Report to author!");
                return;
            }

            OnRoundEnded(leadingTeam);
        }

        private void RoundRestarting() => OnRoundRestarting();

        private void PlayerDied(DiedEventArgs ev)
        {
            if (IsNull(ev.Player) || IsNull(ev.Attacker))
                return;

            OnPlayedDied(ev.Attacker, ev.Player, ev.TargetOldRole);
        }

        private void PlayerUpgradingPickup(UpgradingPickupEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (IsNull(ev.Pickup.PreviousOwner))
                return;

            OnPlayerUpgradedItem(ev.Pickup.PreviousOwner, ev.Pickup.Type.GetCategory());
        }

        private void PlayerUpgradingItem(UpgradingInventoryItemEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            OnPlayerUpgradedItem(ev.Player, ev.Item.Category);
        }

        private void PlayerAddedItem(ItemAddedEventArgs ev)
        {
            if (IsNull(ev.Pickup))
                return;

            OnPlayerPickedUpItem(ev.Player, ev.Item.Category);
        }

        private void PlayerDroppedItem(DroppedItemEventArgs ev)
        {
            if (IsNull(ev.Player) || IsNull(ev.Pickup))
                return;

            OnPlayerDroppedItem(ev.Player, ev.Pickup.Type.GetCategory());
        }

        private void PlayerUsedItem(UsedItemEventArgs ev) => OnPlayerUsedItem(ev.Player, ev.Item.Type);
        private void PlayerSpawned(SpawnedEventArgs ev)
        {
            if (!ev.Player.IsVerified)
                return;
            OnPlayerSpawned(ev.Player);
        }

        private void PlayerEscaping(EscapingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            OnPlayerEscaped(ev.Player);
        }

        private void Scp049FinishingRecall(FinishingRecallEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            OnPlayerResurrected(ev.Player);
        }

        private void PlayerThrown(ThrownProjectileEventArgs ev)
        {
            if (IsNull(ev.Player))
                return;

            TryAddXPAndDisplayMessage(ev.Player, "throw", ev.Projectile.ProjectileType);
        }

        private void PlayerInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (IsNull(ev.Player) || IsNull(ev.Door))
                return;

            TryAddXPAndDisplayMessage(ev.Player, "door", ev.Door.Type);
        }

        private void PlayerActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (IsNull(ev.Player))
                return;

            TryAddXPAndDisplayMessage(ev.Player, "generator", ev.Generator.Room.Type);
        }
    }
#endif
}