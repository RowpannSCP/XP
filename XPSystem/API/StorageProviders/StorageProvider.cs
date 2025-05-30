// ReSharper disable MemberCanBePrivate.Global
namespace XPSystem.API.StorageProviders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders.Models;
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

        public virtual bool TryGetPlayerInfo(IPlayerId playerId, [NotNullWhen(true)] out PlayerInfoWrapper? playerInfo)
        {
            if (TryGetFromCache(playerId, out playerInfo))
                return true;

            bool result = TryGetPlayerInfoNoCache(playerId, out PlayerInfo? playerInfo3);
            playerInfo = playerInfo3!;
            return result;
        }

        public virtual PlayerInfoWrapper GetPlayerInfoAndCreateOfNotExist(IPlayerId playerId)
        {
            if (TryGetFromCache(playerId, out PlayerInfoWrapper? playerInfo))
                return playerInfo;

            return GetPlayerInfoAndCreateOfNotExistNoCache(playerId);
        }

        public virtual void SetPlayerInfo(PlayerInfoWrapper playerInfo)
        {
            if (XPPlayer.TryGetXP(playerInfo.Player, out XPPlayer? player))
                player.Variables.Set(VariableKey, playerInfo);

            SetPlayerInfoNoCache(playerInfo);
        }

        public virtual bool DeletePlayerInfo(IPlayerId playerId)
        {
            if (XPPlayer.TryGetXP(playerId, out XPPlayer? player))
                player.Variables.Remove(VariableKey);

            return DeletePlayerInfoNoCache(playerId);
        }

        public virtual void DeleteAllPlayerInfo()
        {
            ClearCache();
            DeleteAllPlayerInfoNoCache();
        }

        protected virtual bool TryGetFromCache(IPlayerId playerId, [NotNullWhen(true)] out PlayerInfoWrapper? playerInfo)
        {
            playerInfo = null;

            if (!XPPlayer.TryGetXP(playerId, out XPPlayer? player))
            {
                LogDebug("Player not in server: " + playerId);
                playerInfo = null;
                return false;
            }

            if (!player.Variables.TryGet(VariableKey, out object? playerInfoObj))
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
            foreach (BaseXPPlayer player in XPPlayer.PlayersRealConnected)
                player.Variables.Remove(VariableKey);
        }

        public abstract void Initialize();
        public abstract IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count);
        protected abstract bool TryGetPlayerInfoNoCache(IPlayerId playerId, [NotNullWhen(true)] out PlayerInfo? playerInfo);
        protected abstract PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache(IPlayerId playerId);
        protected abstract void SetPlayerInfoNoCache(PlayerInfo playerInfo);
        protected abstract bool DeletePlayerInfoNoCache(IPlayerId playerId);
        protected abstract void DeleteAllPlayerInfoNoCache();

        /// <summary>
        /// Ignore, used by loader in case of <see cref="StorageProvider{T}"/>.
        /// </summary>
        internal virtual void LoadConfig()
        {
        }
    }

    /// <summary>
    /// <see cref="StorageProvider"/> template with config.
    /// </summary>
    /// <typeparam name="T">The type of the config.</typeparam>
    public abstract class StorageProvider<T> : StorageProvider where T : class, new()
    {
        public T Config { get; private set; } = null!;

        /// <summary>
        /// Ignore, used by loader.
        /// </summary>
        internal override void LoadConfig()
        {
            string name = GetType().Name;
            string file = Path.Combine(XPAPI.Config.ExtendedConfigPath, name + ".yml");

            if (File.Exists(file))
            {
                try
                {
                    Config = Deserializer.Deserialize<T>(File.ReadAllText(file));
                }
                catch (Exception e)
                {
                    LogError($"Error loading storageprovider config for {name}: {e}");
                }
            }
            else
            {
                Config = new T();
                File.WriteAllText(file, Serializer.Serialize(Config));
            }
        }
    }
}