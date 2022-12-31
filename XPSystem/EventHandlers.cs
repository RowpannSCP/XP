using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using System.Linq;
using MEC;
using XPSystem.API;

namespace XPSystem
{
    using System.Collections.Generic;
    using Exiled.API.Extensions;
    using Exiled.CustomItems.API.EventArgs;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Scp914;
    using Exiled.Events.EventArgs.Server;
    using PlayerRoles;

    public class EventHandlers
    {
        public Dictionary<Player, List<DoorType>> AlreadyGainedPlayers = new Dictionary<Player, List<DoorType>>();
        public Dictionary<Player, List<ItemCategory>> AlreadyGainedPlayers2 = new Dictionary<Player, List<ItemCategory>>();

        public void OnJoined(VerifiedEventArgs ev)
        {
            if (ev.Player.DoNotTrack)
            {
                ev.Player.OpenReportWindow(Main.Instance.Config.DNTHint);
                return;
            }

            ev.Player.GetLog();
            Timing.CallDelayed(0.5f, () =>
            {
                API.API.UpdateBadge(ev.Player, ev.Player.Group?.BadgeText);
                ev.Player.DisplayNickname = ev.Player.Nickname;
            });
        }

        public void OnKill(DyingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null || ev.Attacker.DoNotTrack)
            {
                return;
            }
            Player killer = ev.DamageHandler.Type == DamageType.PocketDimension ? Player.Get(RoleTypeId.Scp106).FirstOrDefault() : ev.Attacker;
            if (killer == null)
            {
                return;
            }
            if (Main.Instance.Config.KillXP.TryGetValue(killer.Role.Type, out var killxpdict) && killxpdict.TryGetValue(ev.Player.Role, out int xp))
            {
                var log = ev.Attacker.GetLog();
                log.AddXP(xp, Main.GetTranslation("kill"));
                log.UpdateLog();
            }
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            var log = ev.Player.GetLog();
            log.AddXP(Main.Instance.Config.EscapeXP[ev.Player.Role], "escape");
            log.UpdateLog();
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Side team;
            switch (ev.LeadingTeam)
            {
                case LeadingTeam.FacilityForces:
                    team = Side.Mtf;
                    break;
                case LeadingTeam.ChaosInsurgency:
                    team = Side.ChaosInsurgency;
                    break;
                case LeadingTeam.Anomalies:
                    team = Side.Scp;
                    break;
                case LeadingTeam.Draw:
                    team = Side.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var player in Player.Get(team))
            {
                var log = player.GetLog();
                if (log is null)
                    return;
                log.AddXP(Main.Instance.Config.TeamWinXP, Main.GetTranslation("teamwin"));
                log.UpdateLog();
            }
            AlreadyGainedPlayers.Clear();
            AlreadyGainedPlayers2.Clear();
        }

        public void OnLeaving(DestroyingEventArgs ev)
        {
            
        }


        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (Main.Instance.Config.DoorInteractXP.ContainsKey(ev.Door.Type) && Main.Instance.Config.DoorInteractXP[ev.Door.Type] != 0)
            {
                if(!AlreadyGainedPlayers.TryGetValue(ev.Player, out var value))
                {
                    AlreadyGainedPlayers.Add(ev.Player, new List<DoorType>());
                    value = AlreadyGainedPlayers[ev.Player];
                }
                if (!Main.Instance.Config.DoorXPOneTime || !value.Contains(ev.Door.Type))
                {
                    value.Add(ev.Door.Type);
                    var log = ev.Player.GetLog();
                    bool hid = ev.Door.Room.Type == RoomType.HczHid;
                    log.AddXP(Main.Instance.Config.DoorInteractXP[ev.Door.Type], hid ? "doorhid" : Main.GetTranslation(GetDoorKey(ev.Door.Type)));
                }
            }
        }

        private string GetDoorKey(DoorType type) => type switch
        {
            DoorType.Intercom => "doorintercom",
            DoorType.Scp914Gate => "door914",
            DoorType.Scp173Gate => "door173",
            DoorType.GateA => "doorgatea",
            DoorType.GateB => "doorgateb",
            DoorType.Scp049Gate => "door049",
            DoorType.Scp330 => "door330",
            DoorType.CheckpointLczA => "doorcheckpointlcza",
            DoorType.CheckpointLczB => "doorcheckpointlczb",
            DoorType.CheckpointEzHczA => "doorcheckpointhcza",
            DoorType.CheckpointEzHczB => "doorcheckpointhczb",
            _ => "door"
        };

        public void OnScp914UpgradingItem(UpgradingPickupEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (ev.Pickup == null || ev.Pickup.PreviousOwner == null)
                return;
            OnUpgradingItem(ev.Pickup.PreviousOwner, GetCategory(ev.Pickup.Type));
        }

        public void OnScp914UpgradingInventory(UpgradingInventoryItemEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if(ev.Item == null || ev.Player == null)
                return;
            OnUpgradingItem(ev.Player, ev.Item.Category);
        }

        public void OnUpgradingItem(Player ply, ItemCategory type)
        {
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
                    log.AddXP(Main.Instance.Config.UpgradeXP[type], Main.GetTranslation($"upgrading914{type.ToString()}"));
                }
            }
        }

        /// <summary>
        /// If there is an exiled/basegame method for this ping me.
        /// </summary>
        /// <param name="type">type</param>
        private ItemCategory GetCategory(ItemType type)
        {
            if (type.IsAmmo())
                return ItemCategory.Ammo;
            if (type.IsArmor())
                return ItemCategory.Armor;
            if (type.IsKeycard())
                return ItemCategory.Keycard;
            if (type.IsMedical())
                return ItemCategory.Medical;
            if (type.IsScp())
                return ItemCategory.SCPItem;
            if (type.IsWeapon())
                return ItemCategory.Firearm;
            if (type.IsThrowable())
                return ItemCategory.Grenade;
            if (type == ItemType.ParticleDisruptor)
                return ItemCategory.MicroHID;
            if (type == ItemType.Radio)
                return ItemCategory.Radio;
            return ItemCategory.None;
        }
    }
}
