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
        Dictionary<string, string> LoadParameters(FileStream fs);
        void Initialize(Dictionary<string, string> parameters);
        void Dispose();
        bool TryGetPlayerInfo(PlayerId playerId, out PlayerInfo playerInfo);
        PlayerInfo GetPlayerInfoAndCreateOfNotExist(PlayerId playerId);
        IEnumerable<PlayerInfo> GetTopPlayers(int count);
        void SetPlayerInfo(PlayerInfo playerInfo);
        bool DeletePlayerInfo(PlayerId playerId);
        void DeleteAllPlayerInfo();
    }
}