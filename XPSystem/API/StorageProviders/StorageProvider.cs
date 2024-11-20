namespace XPSystem.API.StorageProviders
{
    using System;
    using System.Collections.Generic;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.EventHandlers;
    using static XPAPI;

    /// <summary>
    /// <see cref="IStorageProvider"/> template with caching.
    /// </summary>
    public abstract class StorageProvider : IStorageProvider
    {
        public const string VariableKey = "StorageProviderCache";

        /// <summary>
        /// If you override this, make sure to call base.
        /// </summary>
        public virtual void Dispose() => ClearCache();

        public virtual bool TryGetPlayerInfo(IPlayerId<object> playerId, out PlayerInfoWrapper playerInfo)
        {
            if (TryGetFromCache(playerId, out playerInfo))
                return true;

            bool result = TryGetPlayerInfoNoCache(playerId, out var playerInfo3);
            playerInfo = playerInfo3;
            return result;
        }

        public virtual PlayerInfoWrapper GetPlayerInfoAndCreateOfNotExist(IPlayerId<object> playerId)
        {
            if (TryGetFromCache(playerId, out var playerInfo))
                return playerInfo;

            return GetPlayerInfoAndCreateOfNotExistNoCache(playerId);
        }

        public virtual void SetPlayerInfo(PlayerInfoWrapper playerInfo)
        {
            if (XPPlayer.TryGet(playerInfo.Player, out var player))
                player.Variables.Set(VariableKey, playerInfo);

            SetPlayerInfoNoCache(playerInfo);
        }

        public virtual bool DeletePlayerInfo(IPlayerId<object> playerId)
        {
            if (XPPlayer.TryGet(playerId, out var player))
                player.Variables.Remove(VariableKey);

            return DeletePlayerInfoNoCache(playerId);
        }

        public virtual void DeleteAllPlayerInfo()
        {
            ClearCache();
            DeleteAllPlayerInfoNoCache();
        }

        protected virtual bool TryGetFromCache(IPlayerId<object> playerId, out PlayerInfoWrapper playerInfo)
        {
            playerInfo = null;

            if (!XPPlayer.TryGet(playerId, out var player))
            {
                LogDebug("Player not in server: " + playerId);
                playerInfo = null;
                return false;
            }

            if (!player.Variables.TryGet(VariableKey, out object playerInfoObj))
            {
                LogDebug("Player variable cache not found - adding " + playerId);

                playerInfoObj = new PlayerInfoWrapper(GetPlayerInfoAndCreateOfNotExistNoCache(playerId));
                player.Variables.Set(VariableKey, playerInfoObj);
            }

            if (playerInfoObj is not PlayerInfoWrapper playerInfoWrapper)
            {
                LogDebug("Player variable cache not a PlayerInfoWrapper??? " + playerId);
                return false;
            }

            playerInfo = playerInfoWrapper;
            return true;
        }

        public virtual void ClearCache()
        {
            foreach (var kvp in XPPlayer.Players)
                kvp.Value.Variables.Remove(VariableKey);
        }

        public abstract void Initialize();
        public abstract IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count);
        protected abstract bool TryGetPlayerInfoNoCache(IPlayerId<object> playerId, out PlayerInfo playerInfo);
        protected abstract PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache(IPlayerId<object> playerId);
        protected abstract void SetPlayerInfoNoCache(PlayerInfo playerInfo);
        protected abstract bool DeletePlayerInfoNoCache(IPlayerId<object> playerId);
        protected abstract void DeleteAllPlayerInfoNoCache();

        /// <summary>
        /// Ignore this.
        /// Used by loader when combined with <see cref="StorageProvider{T}"/>.
        /// </summary>
        public virtual object ConfigPropertyInternal { get; set; }

        /// <summary>
        /// Ignore this.
        /// Used by loader when combined with <see cref="StorageProvider{T}"/>.
        /// Type of <see cref="ConfigPropertyInternal"/>.
        /// </summary>
        public virtual Type ConfigTypeInternal { get; } = null;
    }

    /// <summary>
    /// <see cref="StorageProvider"/> template with config.
    /// </summary>
    /// <typeparam name="T">The type of the config.</typeparam>
    public abstract class StorageProvider<T> : StorageProvider where T : new()
    {
        public T Config;

        /// <inheritdoc/>
        public override object ConfigPropertyInternal
        {
            get => Config ?? new T();
            set => Config = (T)value;
        }

        /// <inheritdoc/>
        public override Type ConfigTypeInternal { get; } = typeof(T);
    }
}