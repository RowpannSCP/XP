namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using CommandSystem;
    using Hints;
    using Mirror;
    using RemoteAdmin;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.Config.Models;
    using static LoaderSpecific;

    /// <summary>
    /// <see cref="ReferenceHub"/> wrapper.
    /// <remarks>Multiple instances of this class can exist for the same player, instances are not saved, there is no constructor logic.</remarks>
    /// </summary>
    public class XPPlayer
    {
        private static Dictionary<string, PlayerId> _playerIdCache = new();
        private PlayerId? _playerId;

        /// <summary>
        /// The player's <see cref="ReferenceHub"/>.
        /// </summary>
        public readonly ReferenceHub Hub;

        /// <summary>
        /// Gets the player's user ID.
        /// </summary>
        public string UserId => Hub.authManager.UserId;

        /// <summary>
        /// Gets the player's <see cref="PlayerId"/>.
        /// </summary>
        public PlayerId PlayerId
        {
            get
            {
                if (_playerId != null)
                    return _playerId.Value;

                if (_playerIdCache.TryGetValue(UserId, out var playerId))
                    return (_playerId = playerId).Value;

                if (!XPAPI.TryParseUserId(UserId, out playerId))
                    throw new InvalidOperationException("PlayerId of player is invalid (GetPlayerId).");

                _playerIdCache.Add(UserId, playerId);
                _playerId = playerId;
                return playerId;
            }
        }

        /// <summary>
        /// Gets the player's nickname.
        /// </summary>
        public string Nickname => Hub.nicknameSync.Network_myNickSync;

        /// <summary>
        /// Gets whether or not the player is connected to the server.
        /// </summary>
        public bool IsConnected => Hub.gameObject != null;

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
        public bool HasGlobalBadge => Hub.serverRoles.GlobalSet;

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
        public XPPlayer(ReferenceHub referenceHub)
        {
            if (referenceHub == null)
                throw new System.ArgumentNullException(nameof(referenceHub));
            Hub = referenceHub;
        }

        /// <summary>
        /// Attempts to get a player using a <see cref="PlayerId"/>.
        /// </summary>
        /// <param name="playerId">The <see cref="PlayerId"/> of the player.</param>
        /// <param name="player">The player, if on the server.</param>
        /// <returns>Whether or not the playerid is valid and player is on the server.</returns>
        public static bool TryGetPlayer(PlayerId playerId, out XPPlayer player)
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

            player = new XPPlayer(playerSender.ReferenceHub);
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
        /// Just <see cref="LoaderSpecific.GetHub"/> with useless extra return.
        /// </summary>
        public static bool TryGet(string data, out XPPlayer player)
        {
            player = null;
            if (string.IsNullOrWhiteSpace(data))
                return false;

            var hub = GetHub(data);
            if (hub == null)
                return false;

            player = new XPPlayer(hub);
            return true;
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
                SendFakeSyncVars(typeof(ServerRoles), nameof(ServerRoles.Network_myColor), badge.Color.ToString().ToLower());
            }
            else
            {
                Hub.serverRoles.SetText(badge.Text);
                Hub.serverRoles.SetColor(badge.Color.ToString().ToLower());
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
        public void SendFakeSyncVars(Type targetType, string propertyName, object value)
        {
            if (!IsConnected)
                return;

            var writer = NetworkWriterPool.Get();
            MakeCustomSyncWriter(Hub.networkIdentity, targetType, CustomSyncVarGenerator, writer);

            var message = new EntityStateMessage
            {
                netId = Hub.networkIdentity.netId,
                payload = writer.ToArraySegment(),
            };

            foreach (var referenceHub in ReferenceHub.AllHubs)
            {
                if (referenceHub == Hub)
                    continue;
                referenceHub.connectionToClient.Send(message);
            }

            NetworkWriterPool.Return(writer);

            void CustomSyncVarGenerator(NetworkWriter targetWriter)
            {
                targetWriter.WriteULong(SyncVarDirtyBits[$"{targetType.Name}.{propertyName}"]);
                WriterExtensions[value.GetType()]?.Invoke(null, new[] { targetWriter, value });
            }
        }

        /// <summary>
        /// Send fake values to specific people on the server about something this client owns.
        /// </summary>
        /// <param name="condition">Condition to fulfill to send to.</param>
        /// <param name="targetType"><see cref="NetworkBehaviour"/>'s type.</param>
        /// <param name="propertyName">Property name starting with Network.</param>
        /// <param name="value">Value to send to the people.</param>.
        /// <param name="value2">Value to send to people who don't match the condition.</param>
        public void SendFakeSyncVars(Func<ReferenceHub, bool> condition, Type targetType, string propertyName, object value, object value2 = null)
        {
            if (!IsConnected)
                return;

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

            foreach (var referenceHub in ReferenceHub.AllHubs)
            {
                if (referenceHub == Hub)
                    continue;
                if (condition(referenceHub))
                    referenceHub.connectionToClient.Send(message);
                else if (value2 != null)
                    referenceHub.connectionToClient.Send(message2);
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
        }

        /// <summary>
        /// Force resync to the client's <see cref="SyncVarAttribute"/>.
        /// Can be used to undo <see cref="SendFakeSyncVars(Type, string, object)"/>.
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

        /// <inheritdoc cref="XPPlayer(ReferenceHub)"/>
        public static implicit operator XPPlayer(ReferenceHub hub) => new(hub);

        /// <inheritdoc cref="Hub"/>
        public static implicit operator ReferenceHub(XPPlayer player) => player.Hub;
        
#if EXILED
        /// <inheritdoc cref="Exiled.API.Features.Player"/>
        public static implicit operator XPPlayer(Exiled.API.Features.Player player) => new(player.ReferenceHub);
#else
        public static implicit operator XPPlayer(PluginAPI.Core.Player player) => new(player.ReferenceHub);
#endif
    }
}