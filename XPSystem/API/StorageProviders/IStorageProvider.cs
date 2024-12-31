namespace XPSystem.API.StorageProviders
{
    using System.Collections.Generic;
    using System.IO;
    using XPSystem.API.StorageProviders.Models;

    /// <summary>
    /// Interface for storage providers, basically database connector.
    /// </summary>
    public interface IStorageProvider
    {
        void Initialize();
        void Dispose();
        bool TryGetPlayerInfo(IPlayerId<object> playerId, out PlayerInfoWrapper playerInfo);
        PlayerInfoWrapper GetPlayerInfoAndCreateOfNotExist(IPlayerId<object> playerId);
        IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count);
        void SetPlayerInfo(PlayerInfoWrapper playerInfo);
        bool DeletePlayerInfo(IPlayerId<object> playerId);
        void DeleteAllPlayerInfo();
    }
}