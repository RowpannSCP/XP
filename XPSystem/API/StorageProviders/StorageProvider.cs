namespace XPSystem.API.StorageProviders
{
    using System;
    using System.Collections.Generic;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.EventHandlers;

    /// <summary>
    /// <see cref="IStorageProvider"/> template with caching.
    /// </summary>
    public abstract class StorageProvider : IStorageProvider
    {
        public readonly PlayerInfoCache PlayerInfoCache = new();

        public void Initialize()
        {
            UnifiedEventHandlers.PlayerJoined += PlayerInfoCache.EnsureInCache;
            InitializeNoCache();
        }

        public virtual void Dispose()
        {
            UnifiedEventHandlers.PlayerJoined -= PlayerInfoCache.EnsureInCache;
            PlayerInfoCache.Clear();
            DisposeNoCache();
        }

        public virtual bool TryGetPlayerInfo(PlayerId playerId, out PlayerInfoWrapper playerInfo)
        {
            if (PlayerInfoCache.TryGet(playerId, out playerInfo))
                return true;

            var result = TryGetPlayerInfoNoCache(playerId, out var playerInfo2);
            playerInfo = playerInfo2;
            return result;
        }

        public virtual PlayerInfoWrapper GetPlayerInfoAndCreateOfNotExist(PlayerId playerId)
        {
            if (PlayerInfoCache.TryGet(playerId, out var playerInfo))
                return playerInfo;

            return GetPlayerInfoAndCreateOfNotExistNoCache(playerId);
        }

        public virtual void SetPlayerInfo(PlayerInfoWrapper playerInfo)
        {
            PlayerInfoCache.Update(playerInfo);
            SetPlayerInfoNoCache(playerInfo);
        }

        public virtual bool DeletePlayerInfo(PlayerId playerId)
        {
            PlayerInfoCache.Remove(playerId);
            return DeletePlayerInfoNoCache(playerId);
        }

        public virtual void DeleteAllPlayerInfo()
        {
            PlayerInfoCache.Clear();
            DeleteAllPlayerInfoNoCache();
        }

        public abstract IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count);
        protected abstract void InitializeNoCache();
        protected abstract void DisposeNoCache();
        protected abstract bool TryGetPlayerInfoNoCache(PlayerId playerId, out PlayerInfo playerInfo);
        protected abstract PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache(PlayerId playerId);
        protected abstract void SetPlayerInfoNoCache(PlayerInfo playerInfo);
        protected abstract bool DeletePlayerInfoNoCache(PlayerId playerId);
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
        public virtual Type ConfigTypeInternal => null;
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
        public override Type ConfigTypeInternal => typeof(T);
    }
}