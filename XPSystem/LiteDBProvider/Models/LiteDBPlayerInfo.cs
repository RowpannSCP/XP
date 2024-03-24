namespace XPSystem.LiteDBProvider.Models
{
    using LiteDB;
    using XPSystem.API.Enums;
    using XPSystem.API.StorageProviders.Models;

    public class LiteDBPlayerInfo
    {
        [BsonId]
        public ulong Id { get; set; }
        public int XP { get; set; }
#if STORENICKS
        public string Nickname { get; set; }
#endif

        /// <summary>
        /// Returns a <see cref="PlayerInfo"/> from this <see cref="LiteDBPlayerInfo"/>.
        /// </summary>
        /// <param name="authType">The <see cref="AuthType"/> of the player.</param>
        public PlayerInfo ToPlayerInfo(AuthType authType)
        {
            return new PlayerInfo
            {
                Player = new PlayerId()
                {
                    Id = Id,
                    AuthType = authType
                },
                XP = XP
            };
        }
    }
}