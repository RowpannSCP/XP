namespace XPSystem.API.StorageProviders
{
    /// <summary>
    /// Represents an object of a <see cref="PlayerInfoCache"/>.
    /// </summary>
    public class PlayerInfoCacheObject
    {
        public readonly XPPlayer Player;
        public PlayerInfoWrapper PlayerInfo;

        public bool ShouldPreserve => Player.IsConnected;

        public PlayerInfoCacheObject(PlayerInfoWrapper playerInfo, XPPlayer player)
        {
            PlayerInfo = playerInfo;
            Player = player;
        }
    }
}