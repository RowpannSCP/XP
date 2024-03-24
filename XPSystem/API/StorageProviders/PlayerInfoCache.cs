namespace XPSystem.API.StorageProviders
{
    using System.Collections.Generic;
    using XPSystem.API.StorageProviders.Models;

    /// <summary>
    /// Represents a player info cache that stores playerinfos of currently online players.
    /// </summary>
    public class PlayerInfoCache
    {
        private readonly Dictionary<PlayerId, PlayerInfoCacheObject> _cache = new();

        /// <summary>
        /// Ensures that a player's info is in the cache.
        /// </summary>
        /// <param name="player">The player who the info belongs to.</param>
        /// <param name="playerInfo">The player info to cache.</param>
        public void EnsureInCache(XPPlayer player, PlayerInfo playerInfo)
        {
            _cache[playerInfo.Player] = new PlayerInfoCacheObject(playerInfo, player);
        }

        /// <summary>
        /// Updates the player info entry in the cache.
        /// </summary>
        /// <param name="playerInfo">The player info to update.</param>
        public void Update(PlayerInfo playerInfo)
        {
            if (!_cache.TryGetValue(playerInfo.Player, out var playerInfoCache))
                return;

            playerInfoCache.PlayerInfo = playerInfo;
        }

        /// <summary>
        /// Attempts to get a player info from the cache.
        /// </summary>
        /// <param name="playerId">The player id to get the player info of.</param>
        /// <param name="playerInfo">The player info of the player.</param>
        /// <returns>Whether or not the player info was found in the cache.</returns>
        public bool TryGet(PlayerId playerId, out PlayerInfo playerInfo)
        {
            if (_cache.TryGetValue(playerId, out var cacheObject))
            {
                if (cacheObject.ShouldPreserve)
                {
                    playerInfo = cacheObject.PlayerInfo;
                    return true;
                }

                _cache.Remove(playerId);
            }

            playerInfo = null;
            return false;
        }

        /// <summary>
        /// Removes a player info from the cache.
        /// </summary>
        /// <param name="playerId">The player id of the player info to remove.</param>
        /// <returns>Whether or not a player info was removed.</returns>
        public bool Remove(PlayerId playerId)
        {
            return _cache.Remove(playerId);
        }

        /// <summary>
        /// Clears all entries in the cache.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }
    }
}