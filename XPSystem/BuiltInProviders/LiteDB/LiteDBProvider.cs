namespace XPSystem.BuiltInProviders.LiteDB
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using global::LiteDB;
    using XPSystem.API.Enums;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    public class LiteDBProvider : StorageProvider<LiteDBProvider.LiteDBProviderConfig>
    {
        public ILiteCollection<LiteDBNumberPlayerInfo>? SteamCollection { get; private set; }
        public ILiteCollection<LiteDBNumberPlayerInfo>? DiscordCollection { get; private set; }
        public ILiteCollection<LiteDBStringPlayerInfo>? NWCollection { get; private set; }

        private LiteDatabase? database;

        public override void Initialize()
        {
            database = new LiteDatabase(Config.File);

            SteamCollection = database.GetCollection<LiteDBNumberPlayerInfo>("playerinfo_steam");
            SteamCollection.EnsureIndex(x => x.Id);

            DiscordCollection = database.GetCollection<LiteDBNumberPlayerInfo>("playerinfo_discord");
            DiscordCollection.EnsureIndex(x => x.Id);

            NWCollection = database.GetCollection<LiteDBStringPlayerInfo>("playerinfo_nw");
            NWCollection.EnsureIndex(x => x.Id);

            if (Config.IndexDB)
            {
                SteamCollection.EnsureIndex(x => x.XP);
                DiscordCollection.EnsureIndex(x => x.XP);
                NWCollection.EnsureIndex(x => x.XP);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            SteamCollection = null;
            DiscordCollection = null;
            NWCollection = null;

            database?.Dispose();
            database = null;
        }

        public LiteDBPlayerInfo? TryGetPlayerInfo(IPlayerId playerId) => TryGetPlayerInfo<LiteDBPlayerInfo>(playerId);
        public T? TryGetPlayerInfo<T>(IPlayerId playerId) where T : LiteDBPlayerInfo
        {
            switch (playerId.AuthType)
            {
                // FindById no work; BsonValue converts ulong to decimal, key is different; retarded conversion
                case AuthType.Steam when playerId is NumberPlayerId numberPlayerId:
                    return (T)(object)SteamCollection!.FindOne(x => x.Id == numberPlayerId.IdNumber);
                case AuthType.Discord when playerId is NumberPlayerId numberPlayerId:
                    return (T)(object)DiscordCollection!.FindOne(x => x.Id == numberPlayerId.IdNumber);
                case AuthType.Northwood when playerId is StringPlayerId stringPlayerId:
                    return (T)(object)NWCollection!.FindOne(x => x.Id == stringPlayerId.IdString);
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerId.AuthType), playerId.AuthType, null);
            }
        }

        protected override bool TryGetPlayerInfoNoCache(IPlayerId playerId, [NotNullWhen(true)] out PlayerInfo? playerInfo)
        {
            LiteDBPlayerInfo? existing = TryGetPlayerInfo(playerId);
            if (existing == null)
            {
                playerInfo = null;
                return false;
            }

            playerInfo = existing.ToPlayerInfo(playerId.AuthType);
            return true;
        }

        protected override PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache(IPlayerId playerId)
        {
            LiteDBPlayerInfo? existing = TryGetPlayerInfo(playerId);
            if (existing != null)
                return existing.ToPlayerInfo(playerId.AuthType);

            switch (playerId.AuthType)
            {
                case AuthType.Steam:
                    return GetPlayerInfoAndCreateOfNotExistNoCache(playerId, SteamCollection!);
                case AuthType.Discord:
                    return GetPlayerInfoAndCreateOfNotExistNoCache(playerId, DiscordCollection!);
                case AuthType.Northwood:
                    return GetPlayerInfoAndCreateOfNotExistNoCache(playerId, NWCollection!);
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerId.AuthType), playerId.AuthType, null);
            }
        }
        protected PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache<T>(IPlayerId playerId, ILiteCollection<T> collection) where T : LiteDBPlayerInfo, new()
        {
            T info = new T()
            {
                XP = 0
            }.SetId<T>(playerId);
            collection.Insert(info);
            return info.ToPlayerInfo(playerId.AuthType);
        }

        public override IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count)
        {
            var result = SteamCollection!.Query()
                .OrderByDescending(x => x.XP)
                .Limit(count)
                .ToEnumerable()
                .Select(x => x.ToPlayerInfo(AuthType.Steam))
                .ToList();

            result.AddRange(DiscordCollection!.Query()
                .OrderByDescending(x => x.XP)
                .Limit(count)
                .ToEnumerable()
                .Select(x => x.ToPlayerInfo(AuthType.Discord)));

            result.AddRange(NWCollection!.Query()
                .OrderByDescending(x => x.XP)
                .Limit(count)
                .ToEnumerable()
                .Select(x => x.ToPlayerInfo(AuthType.Northwood)));

            result.Sort((x, y) => y.XP.CompareTo(x.XP));

            return result.Take(count)
                .Select(x => new PlayerInfoWrapper(x));
        }

        protected override void SetPlayerInfoNoCache(PlayerInfo playerInfo)
        {
            switch (playerInfo.Player.AuthType)
            {
                case AuthType.Steam:
                    SetPlayerInfoNoCache(playerInfo, SteamCollection!);
                    break;
                case AuthType.Discord:
                    SetPlayerInfoNoCache(playerInfo, DiscordCollection!);
                    break;
                case AuthType.Northwood:
                    SetPlayerInfoNoCache(playerInfo, NWCollection!);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerInfo.Player.AuthType), playerInfo.Player.AuthType, null);
            }
        }
        protected void SetPlayerInfoNoCache<T>(PlayerInfo playerInfo, ILiteCollection<T> collection) where T : LiteDBPlayerInfo, new()
        {
            T? existing = TryGetPlayerInfo<T>(playerInfo.Player);
            if (existing == null)
            {
                collection.Insert(new T()
                {
                    XP = playerInfo.XP,
#if STORENICKS
                    Nickname = playerInfo.Nickname
#endif
                }.SetId<T>(playerInfo.Player));
            }
            else
            {
                existing.XP = playerInfo.XP;
#if STORENICKS
                existing.Nickname = playerInfo.Nickname;
#endif
                collection.Update(existing);
            }
        }

        protected override bool DeletePlayerInfoNoCache(IPlayerId playerId)
        {
            switch (playerId.AuthType)
            {
                case AuthType.Steam when playerId is NumberPlayerId numberPlayerId:
                    return SteamCollection!.Delete(numberPlayerId.IdNumber);
                case AuthType.Discord when playerId is NumberPlayerId numberPlayerId:
                    return DiscordCollection!.Delete(numberPlayerId.IdNumber);
                case AuthType.Northwood when playerId is StringPlayerId stringPlayerId:
                    return NWCollection!.Delete(stringPlayerId.IdString);
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerId.AuthType), playerId.AuthType, null);
            }
        }

        protected override void DeleteAllPlayerInfoNoCache()
        {
            SteamCollection!.DeleteAll();
            DiscordCollection!.DeleteAll();
            NWCollection!.DeleteAll();
        }

        public class LiteDBProviderConfig
        {
            public bool IndexDB { get; set; } = true;
            public string File { get; set; } = "database.db";
        }
    }
}