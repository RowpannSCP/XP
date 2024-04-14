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
        bool TryGetPlayerInfo(PlayerId playerId, out PlayerInfoWrapper playerInfo);
        PlayerInfoWrapper GetPlayerInfoAndCreateOfNotExist(PlayerId playerId);
        IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count);
        void SetPlayerInfo(PlayerInfoWrapper playerInfo);
        bool DeletePlayerInfo(PlayerId playerId);
        void DeleteAllPlayerInfo();
    }
}