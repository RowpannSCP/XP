namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using CommandSystem;
    using Hints;
    using Mirror;
    using PlayerRoles;
    using RemoteAdmin;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.API.Variables;
    using XPSystem.Config.Models;
    using static LoaderSpecific;

    /// <summary>
    /// <see cref="ReferenceHub"/> wrapper.
    /// </summary>
    /// <remarks>Multiple instances of this class can exist for the same player, instances are not saved, there is no constructor logic.</remarks>
    public partial class XPPlayer
    {
        public static IReadOnlyDictionary<ReferenceHub, XPPlayer> Players => PlayersValue;
        internal static readonly Dictionary<ReferenceHub, XPPlayer> PlayersValue = new();

        /// <summary>
        /// The player's <see cref="ReferenceHub"/>.
        /// </summary>
        public readonly ReferenceHub Hub;

        /// <summary>
        /// Gets the player's user ID.
        /// </summary>
        public string UserId => Hub.authManager.UserId;

        /// <summary>
        /// Gets the player's variables.
        /// </summary>
        public readonly VariableCollection Variables = new();

        /// <summary>
        /// Gets the player's <see cref="PlayerId"/>.
        /// </summary>
        public PlayerId PlayerId { get; }

        /// <summary>
        /// Gets whether or not the player is a npc.
        /// AddXP will return unless forced, if this is true.
        /// </summary>
        public bool IsNPC { get; }

        /// <summary>
        /// Gets the player's nickname.
        /// </summary>
        public string Nickname => Hub.nicknameSync.Network_myNickSync;

        /// <summary>
        /// Gets the player's display name.
        /// </summary>
        public string DisplayName => Hub.nicknameSync.Network_displayName;

        /// <summary>
        /// Gets the name that will be displayed to other players.
        /// </summary>
        public string DisplayedName => DisplayName ?? Nickname;

        /// <summary>
        /// Gets the player's network identity.
        /// </summary>
        public NetworkIdentity NetworkIdentity => Hub.networkIdentity;

        /// <summary>
        /// Gets whether or not the player is connected to the server.
        /// </summary>
        public bool IsConnected => GetIsConnectedSafe();

        /// <summary>
        /// Gets whether or not the player has do not track enabled.
        /// </summary>
        public bool DNT => Hub.authManager.DoNotTrack;

        /// <summary>
        /// Gets the player's group.
        /// </summary>
        public UserGroup Group => Hub.serverRoles.Group; 

        /// <summary>
        /// Gets whether or not the player has a badge.
        /// </summary>
        public bool HasBadge => Group != null;

        /// <summary>
        /// Gets whether or not the player has a hidden badge.
        /// </summary>
        public bool HasHiddenBadge => Hub.serverRoles.HiddenBadge != null;

        /// <summary>
        /// Gets whether or not the player has a global badge.
        /// </summary>
        public bool HasGlobalBadge => !string.IsNullOrEmpty(Hub.serverRoles.NetworkGlobalBadge);

        /// <summary>
        /// Gets the player's badge text.
        /// </summary>
        public string BadgeText => Hub.serverRoles.Network_myText;

        /// <summary>
        /// Gets the player's badge color.
        /// </summary>
        public string BadgeColor => Hub.serverRoles.Network_myColor;

        /// <summary>
        /// Gets whether or not the player can view hidden badges.
        /// </summary>
        public bool CanViewHiddenBadge => HasBadge &&
                                          PermissionsHandler.IsPermitted(Hub.serverRoles.Group.Permissions,
                                              PlayerPermissions.ViewHiddenBadges);

        /// <summary>
        /// Gets the player's current <see cref="RoleTypeId"/>.
        /// </summary>
        public RoleTypeId Role => Hub.roleManager.CurrentRole.RoleTypeId;

        /// <summary>
        /// Gets or sets the player's XP multiplier.
        /// All XP added by methods (all built-in except for directly via non-wrapper) that respect this value will be multiplied by this value.
        /// </summary>
        public float XPMultiplier { get; set; } = 1f;

        public void ShowHint(string message, float duration = 3f)
        {
            Hub.hints.Show(new TextHint(message, new HintParameter[]
            {
                new StringHintParameter(message)
            }, durationScalar: duration));
        }

        public void ShowBroadcast(
            string message,
            ushort duration = 5,
            Broadcast.BroadcastFlags type = Broadcast.BroadcastFlags.Normal,
            bool clearPrevious = false)
        {
            if (clearPrevious)
                Broadcast.Singleton.TargetClearElements(Hub.connectionToClient);
            Broadcast.Singleton.TargetAddElement(Hub.connectionToClient, message, duration, type);
        }

        /// <summary>
        /// Sends a message to the player's client console.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="color">The color of the message.</param>
        public void SendConsoleMessage(string message, string color)
        {
            Hub.gameConsoleTransmission.SendToClient(message, color);
        }

        /// <summary>
        /// Checks if the player has a permission.
        /// </summary>
        /// <param name="permission">The permission to check for.</param>
        /// <returns>Whether or not the player has the permission.</returns>
        public bool CheckPermission(string permission)
        {
            return LoaderSpecific.CheckPermission(Hub, permission);
        }

        /// <summary>
        /// Creates a new instance of <see cref="XPPlayer"/> for the specified <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="referenceHub">The player's <see cref="ReferenceHub"/> to wrap.</param>
        private XPPlayer(ReferenceHub referenceHub)
        {
            if (referenceHub == null)
                throw new ArgumentNullException(nameof(referenceHub));

            Hub = referenceHub;
            IsNPC = CheckNPC(referenceHub);

            if (UserId.TryParseUserId(out var playerId))
            {
                PlayerId = playerId;
            }
            else if (!IsNPC)
            {
                LogWarn("PlayerId "
                        + (string.IsNullOrWhiteSpace(UserId) ? "(empty)" : UserId)
                        + " could not be parsed for player "
                        + DisplayedName);
            }

            PlayersValue.Add(referenceHub, this);
        }

        /// <summary>
        /// Gets a player from their <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> of the player.</param>
        /// <returns>The player.</returns>
        public static XPPlayer Get(ReferenceHub hub)
        {
            if (Players.TryGetValue(hub, out var player))
                return player;

            player = new XPPlayer(hub);
            return player;
        }

        /// <summary>
        /// Attempts to get a player using a <see cref="PlayerId"/>.
        /// </summary>
        /// <param name="playerId">The <see cref="PlayerId"/> of the player.</param>
        /// <param name="player">The player, if on the server.</param>
        /// <returns>Whether or not the playerid is valid and player is on the server.</returns>
        public static bool TryGet(PlayerId playerId, out XPPlayer player)
        {
            return TryGet(playerId.ToString(), out player);
        }

        public static bool TryGet(ICommandSender sender, out XPPlayer player)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                player = null;
                return false;
            }

            player = Get(playerSender.ReferenceHub);
            return true;
        }

        /// <summary>
        /// Just <see cref="LoaderSpecific.GetHub"/> with extra return.
        /// </summary>
        public static bool TryGet(string data, out XPPlayer player)
        {
            player = null;
            if (string.IsNullOrWhiteSpace(data))
                return false;

            var hub = GetHub(data);
            if (hub == null)
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
        public static bool TryGetAndCheckPermission(ICommandSender sender, string permission, out XPPlayer player)
        {
            return TryGet(sender, out player) && player.CheckPermission(permission);
        }

        /// <summary>
        /// Sets the player's badge.
        /// </summary>
        /// <param name="badge">The badge to set.</param>
        /// <param name="fakeSyncVar">Whether or not the badge is actually set on the server and not just synced to the clients.</param>
        public void SetBadge(Badge badge, bool fakeSyncVar)
        {
            if (badge == null)
                throw new ArgumentNullException(nameof(badge));

            if (fakeSyncVar)
            {
                SendFakeSyncVars(typeof(ServerRoles), nameof(ServerRoles.Network_myText), badge.Text);
                SendFakeSyncVars(typeof(ServerRoles), nameof(ServerRoles.Network_myColor), badge.Color);
            }
            else
            {
                Hub.serverRoles.SetText(badge.Text);
                Hub.serverRoles.SetColor(badge.Color);
            }
        }

        /// <summary>
        /// Sets the player's nickname.
        /// </summary>
        /// <param name="nick">The nickname to set.</param>
        /// <param name="fakeSyncVar">Whether or not the nickname is actually set on the server and not just synced to the clients.</param>
        public void SetNick(string nick, bool fakeSyncVar)
        {
            if (string.IsNullOrWhiteSpace(nick))
                throw new ArgumentNullException(nameof(nick));

            if (fakeSyncVar)
            {
                SendFakeSyncVars(typeof(NicknameSync), nameof(NicknameSync.Network_displayName), nick);
            }
            else
            {
                Hub.nicknameSync.DisplayName = nick;
            }
        }

        // Because unity do be like that
        private bool GetIsConnectedSafe()
        {
            try
            {
                return Hub.gameObject != null;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

#region Exiled sync stuff (Source: https://github.com/Exiled-Team/EXILED/blob/master/Exiled.API/Extensions/MirrorExtensions.cs)
        /// <summary>
        /// Send fake values to the client's <see cref="SyncVarAttribute"/>.
        /// </summary>
        /// <param name="behaviorOwner"><see cref="NetworkIdentity"/> of object that owns <see cref="NetworkBehaviour"/>.</param>
        /// <param name="targetType"><see cref="NetworkBehaviour"/>'s type.</param>
        /// <param name="propertyName">Property name starting with Network.</param>
        /// <param name="value">Value to send to target.</param>
        public void SendFakeSyncVar(NetworkIdentity behaviorOwner, Type targetType, string propertyName, object value)
        {
            if (!IsConnected)
                return;

            var writer = NetworkWriterPool.Get();

            MakeCustomSyncWriter(behaviorOwner, targetType, CustomSyncVarGenerator, writer);
            Hub.connectionToClient.Send(new EntityStateMessage
            {
                netId = behaviorOwner.netId,
                payload = writer.ToArraySegment(),
            });

            NetworkWriterPool.Return(writer);

            void CustomSyncVarGenerator(NetworkWriter targetWriter)
            {
                targetWriter.WriteULong(SyncVarDirtyBits[$"{targetType.Name}.{propertyName}"]);
                WriterExtensions[value.GetType()]?.Invoke(null, new[] { targetWriter, value });
            }
        }

        /// <summary>
        /// Send fake values to everyone on the server about something this client owns.
        /// </summary>
        /// <param name="targetType"><see cref="NetworkBehaviour"/>'s type.</param>
        /// <param name="propertyName">Property name starting with Network.</param>
        /// <param name="value">Value to send to everyone.</param>
        /// <param name="skipSelf">Whether or not to skip sending to self.</param>
        /// <returns>The amount of people the packet was sent to.</returns>
        public int SendFakeSyncVars(Type targetType, string propertyName, object value, bool skipSelf = false)
        {
            if (!IsConnected)
                return -1;

            int count = 0;
            var writer = NetworkWriterPool.Get();
            MakeCustomSyncWriter(Hub.networkIdentity, targetType, CustomSyncVarGenerator, writer);

            var message = new EntityStateMessage
            {
                netId = Hub.networkIdentity.netId,
                payload = writer.ToArraySegment(),
            };

            foreach (var referenceHub in ReferenceHub.AllHubs)
            {
                if (skipSelf && referenceHub == Hub)
                    continue;

                referenceHub.connectionToClient.Send(message);
                count++;
            }

            NetworkWriterPool.Return(writer);

            void CustomSyncVarGenerator(NetworkWriter targetWriter)
            {
                targetWriter.WriteULong(SyncVarDirtyBits[$"{targetType.Name}.{propertyName}"]);
                WriterExtensions[value.GetType()]?.Invoke(null, new[] { targetWriter, value });
            }

            return count;
        }

        /// <summary>
        /// Send fake values to specific people on the server about something this client owns.
        /// </summary>
        /// <param name="condition">Condition to fulfill to send to.</param>
        /// <param name="targetType"><see cref="NetworkBehaviour"/>'s type.</param>
        /// <param name="propertyName">Property name starting with Network.</param>
        /// <param name="value">Value to send to the people.</param>.
        /// <param name="value2">Value to send to people who don't match the condition.</param>
        /// <param name="skipSelf">Whether or not to skip sending to self.</param>
        /// <returns>The amount of people the first value was sent to.</returns>
        public int SendFakeSyncVars(Func<XPPlayer, bool> condition, Type targetType, string propertyName, object value, object value2 = null, bool skipSelf = false)
        {
            if (!IsConnected)
                return -1;

            int count = 0;
            var writer = NetworkWriterPool.Get();
            MakeCustomSyncWriter(Hub.networkIdentity, targetType, CustomSyncVarGenerator, writer);

            EntityStateMessage message2 = default;
            if (value2 != null)
            {
                var writer2 = NetworkWriterPool.Get();
                MakeCustomSyncWriter(Hub.networkIdentity, targetType, CustomSyncVarGeneratorValue2, writer2);

                message2 = new EntityStateMessage
                {
                    netId = Hub.networkIdentity.netId,
                    payload = writer2.ToArraySegment(),
                };

                NetworkWriterPool.Return(writer2);
            }

            var message = new EntityStateMessage
            {
                netId = Hub.networkIdentity.netId,
                payload = writer.ToArraySegment(),
            };

            foreach (var kvp in Players)
            {
                if (skipSelf && kvp.Value == this)
                    continue;

                if (condition(kvp.Value))
                {
                    kvp.Value.Hub.connectionToClient.Send(message);
                    count++;
                }
                else if (value2 != null)
                {
                    kvp.Value.Hub.connectionToClient.Send(message2);
                }
            }

            NetworkWriterPool.Return(writer);

            void CustomSyncVarGenerator(NetworkWriter targetWriter)
            {
                targetWriter.WriteULong(SyncVarDirtyBits[$"{targetType.Name}.{propertyName}"]);
                WriterExtensions[value.GetType()]?.Invoke(null, new[] { targetWriter, value });
            }

            void CustomSyncVarGeneratorValue2(NetworkWriter targetWriter)
            {
                targetWriter.WriteULong(SyncVarDirtyBits[$"{targetType.Name}.{propertyName}"]);
                WriterExtensions[value2.GetType()]?.Invoke(null, new[] { targetWriter, value2 });
            }

            return count;
        }

        /// <summary>
        /// Force resync to the client's <see cref="SyncVarAttribute"/>.
        /// Can be used to undo <see cref="SendFakeSyncVars(Type, string, object, bool)"/>.
        /// </summary>
        /// <param name="targetType"><see cref="NetworkBehaviour"/>'s type.</param>
        /// <param name="propertyName">Property name starting with Network.</param>
        public void ResyncSyncVar(Type targetType, string propertyName) =>
            SetDirtyBitsMethodInfo.Invoke(
                Hub.gameObject.GetComponent(targetType), 
                new object[] { SyncVarDirtyBits[$"{targetType.Name}.{propertyName}"] });

        private static void MakeCustomSyncWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncVar, NetworkWriter owner)
        {
            ulong value = 0;
            NetworkBehaviour behaviour = null;

            // Get NetworkBehaviors index (behaviorDirty use index)
            for (int i = 0; i < behaviorOwner.NetworkBehaviours.Length; i++)
            {
                if (behaviorOwner.NetworkBehaviours[i].GetType() == targetType)
                {
                    behaviour = behaviorOwner.NetworkBehaviours[i];
                    value = 1UL << (i & 31);
                    break;
                }
            }

            // Write target NetworkBehavior's dirty
            Compression.CompressVarUInt(owner, value);

            // Write init position
            int position = owner.Position;
            owner.WriteByte(0);
            int position2 = owner.Position;

            // Write sync data
            behaviour!.SerializeObjectsDelta(owner);

            // Write custom syncvar
            customSyncVar?.Invoke(owner);

            // Write syncdata position data
            int position3 = owner.Position;
            owner.Position = position;
            owner.WriteByte((byte)(position3 - position2 & 255));
            owner.Position = position3;
        }
#endregion

        /// <inheritdoc cref="Get(ReferenceHub)"/>
        public static implicit operator XPPlayer(ReferenceHub hub) => Get(hub);

        /// <inheritdoc cref="Hub"/>
        public static implicit operator ReferenceHub(XPPlayer player) => player.Hub;
        
#if EXILED
        /// <inheritdoc cref="Exiled.API.Features.Player"/>
        public static implicit operator XPPlayer(Exiled.API.Features.Player player) => Get(player.ReferenceHub);
#else
        public static implicit operator XPPlayer(PluginAPI.Core.Player player) => Get(player.ReferenceHub);
#endif
    }
}