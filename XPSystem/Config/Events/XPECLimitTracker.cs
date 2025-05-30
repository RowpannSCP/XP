namespace XPSystem.Config.Events
{
    using System;
    using System.Collections.Generic;
    using PlayerRoles;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.Config.Events.Types;
    using XPSystem.Config.Events.Types.Custom;
    using XPSystem.EventHandlers;

    /// <summary>
    /// Tracks limits/cooldowns for events.
    /// Not too proud of this one.
    /// </summary>
    public static class XPECLimitTracker
    {
        const string KeyPrefix = "XPECLimitTracker_";
        static readonly string CooldownKeyPrefix = KeyPrefix + "Cooldown_";
        static readonly string DictKey = KeyPrefix + "Dict";

        /// <summary>
        /// Registers event listeners.
        /// </summary>
        public static void Initialize()
        {
            UnifiedEventHandlers.PlayerChangedRole += OnPlayerChangedRole;
        }

        /// <summary>
        /// Unregisters event listeners.
        /// </summary>
        public static void Disable()
        {
            UnifiedEventHandlers.PlayerChangedRole -= OnPlayerChangedRole;
        }

        /// <summary>
        /// Checks whether or not the player can gain from the given event.
        /// </summary>
        /// <param name="item">The item that the player is trying to use.</param>
        /// <param name="file">The file that the item is from.</param>
        /// <param name="player">The player that is gaining from the event.</param>
        /// <param name="trigger">Whether or not the event limit/cooldowns should be triggered.</param>
        /// <returns>Whether or not the player can use the item.</returns>
        public static bool CanUse(XPECItem item, IXPECLimitedFile file, XPPlayer player, bool trigger)
        {
            if (file.AlwaysAllow)
                return true;

            if (trigger && !file.ZeroXPGainTriggers && item.Amount <= 0)
                trigger = false;

            object obj = file.LimitUnified ? file : item;
            string cooldownKey = CooldownKeyPrefix + obj.GetHashCode();

            if (player.Variables.TryGet(cooldownKey, out _))
                return false;

            var dict = GetDict(player);
            if (dict.TryGetValue(obj, out LimitData data))
            {
                if (data.LeftForLife == 0 || data.LeftForRound == 0)
                    return false;
            }
            else if (trigger)
            {
                data = new LimitData
                {
                    File = file,
                    LeftForLife = file.LifeLimit,
                    LeftForRound = file.RoundLimit
                };

                dict[obj] = data;
            }

            if (trigger)
            {
                if (data.LeftForLife > 0)
                    data.LeftForLife--;

                if (data.LeftForRound > 0)
                    data.LeftForRound--;

                if (data.File.Cooldown > 0)
                    player.Variables.Add(cooldownKey, true, DateTime.Now.AddSeconds(file.Cooldown));
            }

            return true;
        }

        private static void OnPlayerChangedRole(XPPlayer player, RoleTypeId oldRole, RoleTypeId newRole)
        {
            foreach (var kvp in GetDict(player))
            {
                kvp.Value.LeftForLife = kvp.Value.File.LifeLimit;
            }
        }

        private static Dictionary<object, LimitData> GetDict(XPPlayer player)
        {
            if (!player.Variables.TryGet(DictKey, out Dictionary<object, LimitData>? dict))
            {
                dict = new Dictionary<object, LimitData>();
                player.Variables.Add(DictKey, dict);
            }

            return dict;
        }

        private class LimitData
        {
            public IXPECLimitedFile File = null!;

            public int LeftForLife;
            public int LeftForRound;
        }
    }
}