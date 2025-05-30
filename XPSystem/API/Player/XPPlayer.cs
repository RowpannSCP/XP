namespace XPSystem.API.Player
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CommandSystem;
    using RemoteAdmin;
    using XPSystem.API.StorageProviders.Models;
    using static LoaderSpecific;

    /// <summary>
    /// Extended <see cref="BaseXPPlayer"/>, for players that can actually gain XP.
    /// </summary>
    public class XPPlayer : BaseXPPlayer
    {
        public static IEnumerable<BaseXPPlayer> Players => PlayersValue.Values;
        public static IEnumerable<BaseXPPlayer> PlayersRealConnected => PlayersValue.Values
            .Where(x => x.IsConnected && !x.IsNPC);
        public static IEnumerable<XPPlayer> XPPlayers => PlayersRealConnected
            .Where(x => x is XPPlayer)
            .Cast<XPPlayer>();

        internal static readonly Dictionary<ReferenceHub, BaseXPPlayer> PlayersValue = new();

        /// <summary>
        /// Gets the player's <see cref="PlayerId"/>.
        /// </summary>
        public IPlayerId PlayerId { get; }

        /// <summary>
        /// Gets or sets the player's XP multiplier.
        /// All XP added by methods (all built-in except for directly via non-wrapper) that respect this value will be multiplied by this value.
        /// </summary>
        public float XPMultiplier { get; set; } = 1f;

        /// <summary>
        /// Checks if the player can create an <see cref="XPPlayer"/> instance.
        /// This is true if the player is not the host and is not an NPC and does not have DNT enabled.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>Whether or not the player can create an <see cref="XPPlayer"/> instance.</returns>
        private static bool CanCreate(BaseXPPlayer player)
        {
            if (player is not { IsConnected: true })
                return false;
            return player is { IsNPC: false, DNT: false };
        }

        private XPPlayer(BaseXPPlayer player, IPlayerId playerId) : base(player.Hub) => PlayerId = playerId;
        /// <summary>
        /// Attempts to get a player using a <see cref="ReferenceHub"/>. <br/>
        /// Will return a <see cref="BaseXPPlayer"/> if a <see cref="XPPlayer"/> cannot be created (see <see cref="CanCreate"/>).
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> of the player.</param>
        /// <returns>The <see cref="BaseXPPlayer"/> (or derivative) instance of the player.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hub"/> is null.</exception>
        public static BaseXPPlayer Get(ReferenceHub hub)
        {
            if (!hub)
                throw new ArgumentNullException(nameof(hub));

            if (PlayersValue.TryGetValue(hub, out BaseXPPlayer player))
                return player;

            player = new BaseXPPlayer(hub);
            if (CanCreate(player))
            {
                if (player.UserId.TryParseUserId(out IPlayerId? playerId))
                    player = new XPPlayer(player, playerId);
                else
                    LogError("Could not parse user ID of player to player ID: " + player.UserId);
            }

            PlayersValue.Add(hub, player);
            return player;
        }

        /// <summary>
        /// <see cref="Get"/> but with extra cast.
        /// Also won't throw on null hub! (Dummies and patching do that)
        /// </summary>
        public static bool TryGetXP(ReferenceHub? hub, [NotNullWhen(true)] out XPPlayer? player)
        {
            player = null;
            if (!hub)
                return false;

            player = Get(hub) as XPPlayer;
            return player != null;
        }

        /// <summary>
        /// Attempts to get a player using an <see cref="IPlayerId"/>.
        /// </summary>
        /// <param name="playerId">The <see cref="IPlayerId"/> of the player.</param>
        /// <param name="player">The player, if on the server.</param>
        /// <returns>Whether or not the player ID is valid and player is on the server.</returns>
        public static bool TryGet(IPlayerId playerId, [NotNullWhen(true)] out BaseXPPlayer? player)
        {
            return TryGet(playerId.ToString(), out player);
        }

        /// <summary>
        /// <see cref="TryGet(XPSystem.API.StorageProviders.Models.IPlayerId,out XPSystem.API.Player.BaseXPPlayer?)"/> but extra cast.
        /// </summary>
        public static bool TryGetXP(IPlayerId playerId, [NotNullWhen(true)] out XPPlayer? player)
        {
            player = null;
            return TryGet(playerId, out BaseXPPlayer? basePlayer)
                   && basePlayer is XPPlayer xpPlayer
                   && (player = xpPlayer) != null;
        }

        /// <summary>
        /// I wonder...
        /// </summary>
        public static bool TryGet(ICommandSender sender, [NotNullWhen(true)] out BaseXPPlayer? player)
        {
            player = null;
            if (sender is not PlayerCommandSender playerSender)
                return false;

            player = Get(playerSender.ReferenceHub);
            return true;
        }

        /// <summary>
        /// Just <see cref="LoaderSpecific.GetHub"/> with extra conversion.
        /// </summary>
        public static bool TryGet(string data, [NotNullWhen(true)] out BaseXPPlayer? player)
        {
            player = null;
            if (string.IsNullOrWhiteSpace(data))
                return false;

            ReferenceHub? hub = GetHub(data);
            if (!hub)
                return false;

            player = Get(hub);
            return true;
        }

        /// <summary>
        /// Attempts to get a player of a <see cref="ICommandSender"/> and checks if they have a permission.
        /// </summary>
        /// <param name="sender">The sender to get the player from.</param>
        /// <param name="permission">The permission to check for.</param>
        /// <param name="player">The player if found, otherwise null.</param>
        /// <returns>Whether or not the player was found and has the permission.</returns>
        public static bool TryGetAndCheckPermission(ICommandSender sender, string permission, [NotNullWhen(true)] out BaseXPPlayer? player)
        {
            return TryGet(sender, out player) && player.CheckPermission(permission);
        }

        /// <inheritdoc cref="Get(ReferenceHub)"/>
        public static implicit operator XPPlayer?(ReferenceHub hub) => Get(hub) as XPPlayer;

        /// <inheritdoc cref="BaseXPPlayer.Hub"/>
        public static implicit operator ReferenceHub(XPPlayer player) => player.Hub;

#if EXILED
        /// <inheritdoc cref="Get(ReferenceHub)"/>
        public static implicit operator XPPlayer?(Exiled.API.Features.Player player) => Get(player.ReferenceHub) as XPPlayer;
#else
        /// <inheritdoc cref="Get(ReferenceHub)"/>
        public static implicit operator XPPlayer?(LabApi.Features.Wrappers.Player player) => Get(player.ReferenceHub) as XPPlayer;
#endif
    }
}