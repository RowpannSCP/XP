using MEC;
using XPSystem.API;

namespace XPSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using InventorySystem.Items;
    using InventorySystem.Items.Pickups;
    using PlayerRoles;
    using PlayerStatsSystem;
    using Scp914;
    using UnityEngine;

    public class EventHandlers
    {
#if EXILED
        public Dictionary<Exiled.API.Features.Player, List<Exiled.API.Enums.DoorType>> AlreadyGainedPlayers = new Dictionary<Exiled.API.Features.Player, List<Exiled.API.Enums.DoorType>>();
#endif
        public Dictionary<ReferenceHub, List<ItemCategory>> AlreadyGainedPlayers2 = new Dictionary<ReferenceHub, List<ItemCategory>>();
        public Dictionary<ReferenceHub, List<ItemType>> AlreadyGainedPlayers3 = new Dictionary<ReferenceHub, List<ItemType>>();
        //public Dictionary<Player, List<ItemCategory>> AlreadyGainedPlayers4 = new Dictionary<Player, List<ItemCategory>>();
        public Dictionary<ReferenceHub, List<ItemType>> AlreadyGainedPlayers5 = new Dictionary<ReferenceHub, List<ItemType>>();
        public Dictionary<ReferenceHub, List<ItemType>> AlreadyGainedPlayers6 = new Dictionary<ReferenceHub, List<ItemType>>();

#if EXILED
        public void OnJoined(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
            var hub = ev.Player.ReferenceHub;
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerJoined)]
        public void OnJoined(PluginAPI.Core.Player ply)
        {
            var hub = ply.ReferenceHub;
#endif
            if (hub.serverRoles.DoNotTrack)
            {
                hub.characterClassManager.ConsolePrint($"[REPORTING] {Main.Instance.Config.DNTHint}", "white");
                return;
            }

            hub.GetLog();
            Timing.CallDelayed(0.5f, () =>
            {
                if (Main.EnabledRank)
                    API.API.UpdateBadge(hub, hub.serverRoles.Group?.BadgeText);
                if (Main.EnabledNick)
                    hub.nicknameSync.DisplayName = hub.nicknameSync.Network_myNickSync;
            });
        }

#if EXILED
        public void OnKill(Exiled.Events.EventArgs.Player.DiedEventArgs ev)
        {
            var hub = ev.Player?.ReferenceHub;
            var attackerHub = ev.Attacker?.ReferenceHub;
            var damageHandler = ev.DamageHandler.Base;
            var oldRole = ev.TargetOldRole;
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerDying)]
        public void OnKill(PluginAPI.Core.Player player, PluginAPI.Core.Player attacker, DamageHandlerBase damageHandler)
        {
            var hub = player?.ReferenceHub;
            var attackerHub = attacker?.ReferenceHub;
            RoleTypeId oldRole = RoleTypeId.None;
            if (player != null)
                oldRole = player.Role;
#endif
            if (Main.Paused) return;
            if (hub == null || (attackerHub != null && attackerHub.serverRoles.DoNotTrack))
            {
                return;
            }
            Main.DebugProgress("OnKillPre");

            if (damageHandler is UniversalDamageHandler universalDamageHandler &&
                universalDamageHandler.TranslationId == DeathTranslations.PocketDecay.Id)
                attackerHub ??= ReferenceHub.AllHubs.FirstOrDefault(x => x.GetRoleId() == RoleTypeId.Scp106);
            if (attackerHub == null)
            {
                return;
            }

            var attackerRoleId = attackerHub.GetRoleId();
            var hasKey = Main.Instance.Config.KillXP.TryGetValue(attackerRoleId, out var killxpdict);
            if (!hasKey)
                hasKey = Main.Instance.Config.KillXP.TryGetValue(RoleTypeId.None, out killxpdict);
            Main.DebugProgress($"OnKill, haskey: {hasKey} (attroleid: {attackerRoleId}) (role: {oldRole})");
            if (hasKey && (killxpdict.TryGetValue(oldRole, out int xp) || killxpdict.TryGetValue(RoleTypeId.None, out xp)))
            {
                var log = attackerHub.GetLog();
                log.AddXP(xp, Main.GetTranslation($"kill{attackerRoleId.ToString()}"));
            }
        }

#if EXILED
        public void OnEscape(Exiled.Events.EventArgs.Player.EscapingEventArgs ev)
        {
            var hub = ev.Player.ReferenceHub;
            var role = ev.NewRole;
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerEscape)]
        public void OnEscape(PluginAPI.Core.Player ply, RoleTypeId role)
        {
            var hub = ply.ReferenceHub;
#endif
            if (Main.Paused) return;
            if (hub.serverRoles.DoNotTrack)
                return;
            if (!Main.Instance.Config.EscapeXP.TryGetValue(hub.GetRoleId(), out int xp))
            {
                Main.LogWarn($"No escape XP for {hub.GetRoleId()}");
                return;
            }
            var log = hub.GetLog();
            log.AddXP(xp, Main.GetTranslation("escape"));
        }

#if EXILED
        public void OnRoundEnd(Exiled.Events.EventArgs.Server.RoundEndedEventArgs ev)
        {
            var team = (RoundSummary.LeadingTeam)Enum.Parse(typeof(RoundSummary.LeadingTeam), ev.LeadingTeam.ToString());
            Extensions._hintQueue.Clear();
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.RoundEnd)]
        public void OnRoundEnd(RoundSummary.LeadingTeam team)
        {
#endif
            if (Main.Paused) return;
            foreach (var hub in ReferenceHub.AllHubs)
            {
                if(hub == ReferenceHub.HostHub)
                    continue;
                if (hub.serverRoles.DoNotTrack)
                    continue;
                if (!Winners[team].Contains(hub.GetRoleId()))
                {
                    continue;
                }
                var log = hub.GetLog();
                if (log is null)
                    continue;
                log.AddXP(Main.Instance.Config.TeamWinXP, Main.GetTranslation("teamwin"));
            }
#if EXILED
            AlreadyGainedPlayers.Clear();
#endif
            AlreadyGainedPlayers2.Clear();
            AlreadyGainedPlayers3.Clear();
            //AlreadyGainedPlayers4.Clear();
            AlreadyGainedPlayers5.Clear();
            AlreadyGainedPlayers6.Clear();
        }

        private static Dictionary<RoundSummary.LeadingTeam, List<RoleTypeId>> Winners =
            new Dictionary<RoundSummary.LeadingTeam, List<RoleTypeId>>()
            {
                [RoundSummary.LeadingTeam.Anomalies] = new List<RoleTypeId>()
                {
                    RoleTypeId.Scp049,
                    RoleTypeId.Scp079,
                    RoleTypeId.Scp096,
                    RoleTypeId.Scp106,
                    RoleTypeId.Scp173,
                    RoleTypeId.Scp0492,
                    RoleTypeId.Scp939,
                },
                [RoundSummary.LeadingTeam.ChaosInsurgency] = new List<RoleTypeId>()
                {
                    RoleTypeId.ChaosConscript,
                    RoleTypeId.ChaosMarauder,
                    RoleTypeId.ChaosRepressor,
                    RoleTypeId.ChaosRifleman,
                    RoleTypeId.ClassD
                },
                [RoundSummary.LeadingTeam.FacilityForces] = new List<RoleTypeId>()
                {
                    RoleTypeId.NtfCaptain,
                    RoleTypeId.NtfPrivate,
                    RoleTypeId.NtfSergeant,
                    RoleTypeId.NtfSpecialist,
                    RoleTypeId.Scientist,
                    RoleTypeId.FacilityGuard
                },
                [RoundSummary.LeadingTeam.Draw] = new List<RoleTypeId>()
            };

#if EXILED
        public void OnInteractingDoor(Exiled.Events.EventArgs.Player.InteractingDoorEventArgs ev)
        {
            if (Main.Paused) return;
            if (!ev.IsAllowed)
                return;
            if (ev.Player.DoNotTrack)
                return;
            if (ev.Player.CurrentItem is null && Main.Instance.Config.DontGiveDoorXPEmptyItem)
                return;
            if (Main.Instance.Config.DoorInteractXP.ContainsKey(ev.Door.Type) && Main.Instance.Config.DoorInteractXP[ev.Door.Type] != 0)
            {
                if(!AlreadyGainedPlayers.TryGetValue(ev.Player, out var value))
                {
                    AlreadyGainedPlayers.Add(ev.Player, new List<Exiled.API.Enums.DoorType>());
                    value = AlreadyGainedPlayers[ev.Player];
                }
                if (!Main.Instance.Config.DoorXPOneTime || !value.Contains(ev.Door.Type))
                {
                    value.Add(ev.Door.Type);
                    var log = ev.Player.ReferenceHub.GetLog();
                    bool hid = ev.Door.Room.Type == Exiled.API.Enums.RoomType.HczHid;
                    log.AddXP(Main.Instance.Config.DoorInteractXP[ev.Door.Type], hid ? "doorhid" : Main.GetTranslation(GetDoorKey(ev.Door.Type)));
                }
            }
        }

        private static string GetDoorKey(Exiled.API.Enums.DoorType type) => "door" + type;

        public void OnScp914UpgradingItem(Exiled.Events.EventArgs.Scp914.UpgradingPickupEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (ev.Pickup == null || ev.Pickup.PreviousOwner == null)
                return;
            OnUpgradingItem(ev.Pickup.PreviousOwner.ReferenceHub, GetCategory(ev.Pickup.Type));
        }

        public void OnScp914UpgradingInventory(Exiled.Events.EventArgs.Scp914.UpgradingInventoryItemEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if(ev.Item == null || ev.Player == null)
                return;
            OnUpgradingItem(ev.Player.ReferenceHub, ev.Item.Category);
        }
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.Scp914UpgradePickup)]
        public void OnScp914UpgradingItem(ItemPickupBase item, Vector3 outPos, Scp914KnobSetting setting)
        {
            if (item == null || item.PreviousOwner.Hub == null)
                return;
            OnUpgradingItem(item.PreviousOwner.Hub, GetCategory(item.Info.ItemId));
        }

        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.Scp914UpgradeInventory)]
        public void OnScp914UpgradingInventory(PluginAPI.Core.Player player, ItemBase item, Scp914KnobSetting setting)
        {
            if(item == null || player == null)
                return;
            OnUpgradingItem(player.ReferenceHub, item.Category);
        }
#endif

        public void OnUpgradingItem(ReferenceHub ply, ItemCategory type)
        {
            if (Main.Paused) return;
            if (ply.serverRoles.DoNotTrack)
                return;
            if (Main.Instance.Config.UpgradeXP.ContainsKey(type) && Main.Instance.Config.UpgradeXP[type] != 0)
            {
                if(!AlreadyGainedPlayers2.TryGetValue(ply, out var value))
                {
                    AlreadyGainedPlayers2.Add(ply, new List<ItemCategory>());
                    value = AlreadyGainedPlayers2[ply];
                }
                if (!Main.Instance.Config.UpgradeXPOneTime || !value.Contains(type))
                {
                    value.Add(type);
                    var log = ply.GetLog();
                    log.AddXP(Main.Instance.Config.UpgradeXP[type], 
                        Main.GetTranslation($"upgrading914{type.ToString()}")
                        ?? Main.GetTranslation("upgrading914"));
                }
            }
        }

        /// <summary>
        /// If there is an exiled/basegame method for this ping me.
        /// </summary>
        /// <param name="type">type</param>
        private ItemCategory GetCategory(ItemType type)
        {
            if (type is ItemType.Ammo9x19 or ItemType.Ammo12gauge or ItemType.Ammo44cal or ItemType.Ammo556x45 or ItemType.Ammo762x39)
                return ItemCategory.Ammo;
            if (type is ItemType.ArmorCombat or ItemType.ArmorHeavy or ItemType.ArmorLight)
                return ItemCategory.Armor;
            if (type is ItemType.KeycardJanitor or ItemType.KeycardScientist or
                ItemType.KeycardResearchCoordinator or ItemType.KeycardZoneManager or ItemType.KeycardGuard or ItemType.KeycardNTFOfficer or
                ItemType.KeycardContainmentEngineer or ItemType.KeycardNTFLieutenant or ItemType.KeycardNTFCommander or
                ItemType.KeycardFacilityManager or ItemType.KeycardChaosInsurgency or ItemType.KeycardO5)
                return ItemCategory.Keycard;
            if (type is ItemType.Painkillers or ItemType.Medkit or ItemType.SCP500 or ItemType.Adrenaline)
                return ItemCategory.Medical;
            if (type is ItemType.SCP207 or ItemType.SCP244a or ItemType.SCP244b or ItemType.SCP268 or ItemType.SCP330 or ItemType.SCP1576 or ItemType.SCP1853)
                return ItemCategory.SCPItem;
            if (type is ItemType.GunCrossvec or ItemType.GunCom45 or ItemType.GunLogicer or ItemType.GunRevolver or ItemType.GunShotgun or ItemType.GunAK
                or ItemType.GunCOM15 or ItemType.GunCOM18 or ItemType.GunE11SR or ItemType.GunFSP9
                or ItemType.ParticleDisruptor)
                return ItemCategory.Firearm;
            if (type is ItemType.SCP018 or ItemType.GrenadeHE or ItemType.GrenadeFlash or ItemType.SCP2176)
                return ItemCategory.Grenade;
            if (type == ItemType.MicroHID)
                return ItemCategory.MicroHID;
            if (type == ItemType.Radio)
                return ItemCategory.Radio;
            return ItemCategory.None;
        }

#if EXILED
        public void OnSpawning(Exiled.Events.EventArgs.Player.SpawnedEventArgs ev)
        {
            var hub = ev.Player?.ReferenceHub;
            if (hub == null)
                return;
            var roleType = ev.Player.Role.Type;
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerSpawn)]
        public void Spawned(PluginAPI.Core.Player ply, RoleTypeId roleType)
        {
            var hub = ply?.ReferenceHub;
            if (hub == null)
                return;
#endif
            if (Main.Paused) return;
            Timing.CallDelayed(1f, () =>
            {
                var log = hub.GetLog();
                if (Main.Instance.Config.SpawnXP.TryGetValue(roleType, out var value) || Main.Instance.Config.SpawnXP.TryGetValue(RoleTypeId.None, out value))
                    log.AddXP(value, Main.GetTranslation($"spawned{roleType.ToString()}"));
                else if (Main.Instance.Config.Debug)
                    Main.LogWarn("Skipping spawn xp for " + roleType + " since there was not amount defined");
            });
        }

#if EXILED
        public void OnPickingUpItem(Exiled.Events.EventArgs.Player.PickingUpItemEventArgs ev)
        {
            if (!Main.Instance.CanGetXP(ev.Player, "pickup", ev.Pickup?.Serial ?? 0))
                return;
            var hub = ev.Player.ReferenceHub;
            var pickup = ev.Pickup.Base;
#else
		[PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerSearchedPickup)]
		void OnSearchedPickup(PluginAPI.Core.Player ply, ItemPickupBase pickup)
		{
            if (!Main.Instance.CanGetXP(ply, "pickup", pickup.Info.Serial, pickup.Info.ItemId))
                return;
            var hub = ply?.ReferenceHub;
#endif
            if (Main.Paused) return;
            if(pickup == null)
                return;

            if (Main.Instance.Config.PickupXPOneTime)
            {
                if(!AlreadyGainedPlayers3.ContainsKey(hub))
                    AlreadyGainedPlayers3.Add(hub, new List<ItemType>());
                if (AlreadyGainedPlayers3.TryGetValue(hub, out var list))
                {
                    if (!list.Contains(pickup.Info.ItemId))
                    {
                        list.Add(pickup.Info.ItemId);
                        HandlePickup(hub, pickup);
                    }
                }
                return;
            }

            HandlePickup(hub, pickup);
        }

        void HandlePickup(ReferenceHub ply, ItemPickupBase pickupBase)
        {
            var log = ply.GetLog();
            if(Main.Instance.Config.PickupXP.TryGetValue(pickupBase.Info.ItemId, out var value) || Main.Instance.Config.PickupXP.TryGetValue(ItemType.None, out value))
                log.AddXP(value, Main.GetTranslation($"pickup{pickupBase.Info.ItemId.ToString()}"));
        }

#if EXILED
        public void OnThrowingGrenade(Exiled.Events.EventArgs.Player.ThrownProjectileEventArgs ev)
        {
            if (Main.Paused) return;
            var log = ev.Player.ReferenceHub.GetLog();
            if(Main.Instance.Config.ThrowXP.TryGetValue(ev.Projectile.ProjectileType, out var value))
                log.AddXP(value, Main.GetTranslation($"throw{ev.Projectile.ProjectileType.ToString()}"));
        }
#endif

#if EXILED
        public void OnDroppingItem(Exiled.Events.EventArgs.Player.DroppingItemEventArgs ev)
        {
            if (!Main.Instance.CanGetXP(ev.Player, "drop", ev.Item.Serial))
                return;
            var hub = ev.Player.ReferenceHub;
            var itemType = ev.Item.Type;
#else
        [PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerDropItem)]
		void OnPlayerDroppedItem(PluginAPI.Core.Player ply, ItemBase item)
		{
            if (!Main.Instance.CanGetXP(ply, "drop", item.ItemSerial, item.ItemTypeId))
                return;
            var hub = ply?.ReferenceHub;
            var itemType = item.ItemTypeId;
#endif
            if (Main.Paused) return;
            var log = hub.GetLog();
            if (Main.Instance.Config.DropXPOneTime)
            {
                if(!AlreadyGainedPlayers5.ContainsKey(hub))
                    AlreadyGainedPlayers5.Add(hub, new List<ItemType>());
                if (AlreadyGainedPlayers5.TryGetValue(hub, out var list))
                {
                    if (!list.Contains(itemType))
                    {
                        list.Add(itemType);
                        if(Main.Instance.Config.DropXP.TryGetValue(itemType, out var value) || Main.Instance.Config.DropXP.TryGetValue(ItemType.None, out value))
                            log.AddXP(value, Main.GetTranslation($"drop{itemType.ToString()}"));
                    }
                }
                return;
            }
            if(Main.Instance.Config.DropXP.TryGetValue(itemType, out var value2) || Main.Instance.Config.DropXP.TryGetValue(ItemType.None, out value2))
                log.AddXP(value2, Main.GetTranslation($"drop{itemType.ToString()}"));
        }

#if EXILED
        public void OnUsingItem(Exiled.Events.EventArgs.Player.UsedItemEventArgs ev)
        {
            var hub = ev.Player.ReferenceHub;
            var itemType = ev.Item.Type;
#else
		[PluginAPI.Core.Attributes.PluginEvent(PluginAPI.Enums.ServerEventType.PlayerUsedItem)]
		void OnPlayerUsedItem(PluginAPI.Core.Player ply, ItemBase item)
		{
            var hub = ply?.ReferenceHub;
            var itemType = item.ItemTypeId;
#endif
            if (Main.Paused) return;
            var log = hub.GetLog();
            if (Main.Instance.Config.UseXPOneTime)
            {
                if(!AlreadyGainedPlayers6.ContainsKey(hub))
                    AlreadyGainedPlayers6.Add(hub, new List<ItemType>());
                if (AlreadyGainedPlayers6.TryGetValue(hub, out var list))
                {
                    if (!list.Contains(itemType))
                    {
                        list.Add(itemType);
                        if(Main.Instance.Config.UseXP.TryGetValue(itemType, out var value) || Main.Instance.Config.UseXP.TryGetValue(ItemType.None, out value))
                            log.AddXP(value, Main.GetTranslation($"use{itemType.ToString()}"));
                    }
                }
                return;
            }
            if(Main.Instance.Config.UseXP.TryGetValue(itemType, out var value2) || Main.Instance.Config.UseXP.TryGetValue(ItemType.None, out value2))
                log.AddXP(value2, Main.GetTranslation($"use{itemType.ToString()}"));
        }
    }
}
