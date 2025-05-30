namespace XPSystem.API.Player
{
    using System;
    using Hints;
    using Mirror;
    using PlayerRoles;
    using XPSystem.API.Variables;
    using static LoaderSpecific;

    /// <summary>
    /// <see cref="ReferenceHub"/> wrapper. Kinda like the one found in plugin frameworks.
    /// </summary>
    public class BaseXPPlayer
    {
        /// <summary>
        /// The player's <see cref="ReferenceHub"/>.
        /// </summary>
        public readonly ReferenceHub Hub;
        internal BaseXPPlayer(ReferenceHub hub) => Hub = hub;

        /// <summary>
        /// Gets the player's user ID.
        /// </summary>
        public string UserId => Hub.authManager.UserId;

        /// <summary>
        /// Gets the player's variables.
        /// </summary>
        public readonly VariableCollection Variables = new();

        /// <summary>
        /// Gets the player's nickname.
        /// </summary>
        public string Nickname => Hub.nicknameSync.Network_myNickSync;

        /// <summary>
        /// Gets the player's display name.
        /// </summary>
        public string? DisplayName => Hub.nicknameSync.Network_displayName;

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
        public bool IsConnected => Hub && Hub.gameObject;

        /// <summary>
        /// Gets whether or not the player is a NPC.
        /// </summary>
        public bool IsNPC => Hub.IsHost || CheckNPC(Hub);

        /// <summary>
        /// Gets whether or not the player has do not track enabled.
        /// Should always be false unless the creation was forced (which you shouldn't do).
        /// </summary>
        public bool DNT => Hub.authManager.DoNotTrack;

        /// <summary>
        /// Gets the player's group.
        /// </summary>
        public UserGroup? Group => Hub.serverRoles.Group; 

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
        public string? BadgeText => Hub.serverRoles.Network_myText;

        /// <summary>
        /// Gets the player's badge color.
        /// </summary>
        public string? BadgeColor => Hub.serverRoles.Network_myColor;

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
        /// Gets the <see cref="RoundSummary.LeadingTeam"/> of the player's team.
        /// </summary>
        public RoundSummary.LeadingTeam LeadingTeam => Role.GetTeam().GetLeadingTeam();

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

            NetworkWriterPooled writer = NetworkWriterPool.Get();

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
            NetworkWriterPooled writer = NetworkWriterPool.Get();
            MakeCustomSyncWriter(Hub.networkIdentity, targetType, CustomSyncVarGenerator, writer);

            var message = new EntityStateMessage
            {
                netId = Hub.networkIdentity.netId,
                payload = writer.ToArraySegment(),
            };

            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (skipSelf && hub == Hub)
                    continue;

                hub.connectionToClient.Send(message);
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
        public int SendFakeSyncVars(Func<BaseXPPlayer, bool> condition, Type targetType, string propertyName, object value, object value2 = null!, bool skipSelf = false)
        {
            if (!IsConnected)
                return -1;

            int count = 0;
            NetworkWriterPooled writer = NetworkWriterPool.Get();
            MakeCustomSyncWriter(Hub.networkIdentity, targetType, CustomSyncVarGenerator, writer);

            EntityStateMessage message2 = default;
            if (value2 != null!)
            {
                NetworkWriterPooled writer2 = NetworkWriterPool.Get();
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

            foreach (BaseXPPlayer player in XPPlayer.PlayersRealConnected)
            {
                if (skipSelf && player == this)
                    continue;

                if (condition(player))
                {
                    player.Hub.connectionToClient.Send(message);
                    count++;
                }
                else if (value2 != null)
                {
                    player.Hub.connectionToClient.Send(message2);
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

        private static void MakeCustomSyncWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter>? customSyncVar, NetworkWriter owner)
        {
            ulong value = 0;
            NetworkBehaviour behaviour = null!;

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

        /// <inheritdoc cref="XPPlayer.Get(ReferenceHub)"/>
        public static implicit operator BaseXPPlayer(ReferenceHub hub) => XPPlayer.Get(hub);
        /// <inheritdoc cref="Hub"/>
        public static implicit operator ReferenceHub(BaseXPPlayer player) => player.Hub;

#if EXILED
        /// <inheritdoc cref="XPPlayer.Get(ReferenceHub)"/>
        public static implicit operator BaseXPPlayer(Exiled.API.Features.Player player) => XPPlayer.Get(player.ReferenceHub);
#else
        /// <inheritdoc cref="XPPlayer.Get(ReferenceHub)"/>
        public static implicit operator BaseXPPlayer(LabApi.Features.Wrappers.Player player) => Get(player.ReferenceHub);
#endif
    }
}