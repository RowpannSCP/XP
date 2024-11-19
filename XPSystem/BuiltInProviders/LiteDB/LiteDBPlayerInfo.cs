namespace XPSystem.BuiltInProviders.LiteDB
{
    using global::LiteDB;
    using XPSystem.API.Enums;
    using XPSystem.API.StorageProviders.Models;

    public class LiteDBNumberPlayerInfo : LiteDBPlayerInfo
    {
        [BsonId]
        public ulong Id { get; set; }

        protected override IPlayerId toPlayerId(AuthType authType) => new NumberPlayerId(Id, authType);
    }

    public class LiteDBStringPlayerInfo : LiteDBPlayerInfo
    {
        [BsonId]
        public string Id { get; set; }

        protected override IPlayerId toPlayerId(AuthType authType) => new StringPlayerId(Id, authType);
    }

    public abstract class LiteDBPlayerInfo
    {
        public int XP { get; set; }
#if STORENICKS
        public string Nickname { get; set; }
#endif

        protected abstract IPlayerId toPlayerId(AuthType authType);
        public PlayerInfo ToPlayerInfo(AuthType authType)
        {
            return new PlayerInfo
            {
                Player = toPlayerId(authType),
                XP = XP,
#if STORENICKS
                Nickname = Nickname
#endif
            };
        }
    }
}