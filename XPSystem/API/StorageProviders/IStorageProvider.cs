namespace XPSystem.API.StorageProviders
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using XPSystem.API.StorageProviders.Models;

    /// <summary>
    /// Interface for storage providers, basically database connector.
    /// </summary>
    public interface IStorageProvider
    {
        void Initialize();
        void Dispose();
        bool TryGetPlayerInfo(IPlayerId playerId, [NotNullWhen(true)] out PlayerInfoWrapper? playerInfo);
        PlayerInfoWrapper GetPlayerInfoAndCreateOfNotExist(IPlayerId playerId);
        IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count);
        void SetPlayerInfo(PlayerInfoWrapper playerInfo);
        bool DeletePlayerInfo(IPlayerId playerId);
        void DeleteAllPlayerInfo();
    }
}