namespace XPSystem.API.StorageProviders
{
    using XPSystem.API.StorageProviders.Models;

    /// <summary>
    /// Represents an object of a <see cref="PlayerInfoCache"/>.
    /// </summary>
    public class PlayerInfoCacheObject
    {
        public readonly XPPlayer Player;
        public PlayerInfo PlayerInfo;

        public bool ShouldPreserve => Player.IsConnected;

        public PlayerInfoCacheObject(PlayerInfo playerInfo, XPPlayer player)
        {
            PlayerInfo = playerInfo;
            Player = player;
        }
    }
}