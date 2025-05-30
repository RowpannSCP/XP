namespace XPSystem.BuiltInProviders.LiteDB
{
    using System;
    using global::LiteDB;
    using XPSystem.API.Enums;
    using XPSystem.API.StorageProviders.Models;

    public class LiteDBNumberPlayerInfo : LiteDBPlayerInfo
    {
        [BsonId]
        public ulong Id { get; set; }

        public override T SetId<T>(IPlayerId id)
        {
            if (id is not NumberPlayerId numberPlayerId)
                throw new ArgumentException("id is not NumberPlayerId", nameof(id));

            Id = numberPlayerId.IdNumber;
            return (T)(object)this;
        }

        protected override IPlayerId toPlayerId(AuthType authType) => new NumberPlayerId(Id, authType);
    }

    public class LiteDBStringPlayerInfo : LiteDBPlayerInfo
    {
        [BsonId]
        public string Id { get; set; } = null!;

        public override T SetId<T>(IPlayerId id)
        {
            if (id is not StringPlayerId stringPlayerId)
                throw new ArgumentException("id is not StringPlayerId", nameof(id));

            Id = stringPlayerId.IdString;
            return (T)(object)this;
        }

        protected override IPlayerId toPlayerId(AuthType authType) => new StringPlayerId(Id, authType);
    }

    public abstract class LiteDBPlayerInfo
    {
        public int XP { get; set; }
#if STORENICKS
        public string? Nickname { get; set; }
#endif

        public abstract T SetId<T>(IPlayerId id) where T : LiteDBPlayerInfo;

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