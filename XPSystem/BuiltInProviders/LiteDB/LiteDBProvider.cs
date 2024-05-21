﻿namespace XPSystem.BuiltInProviders.LiteDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::LiteDB;
    using XPSystem.API.Enums;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    public class LiteDBProvider : StorageProvider<LiteDBProvider.LiteDBProviderConfig>
    {
        public ILiteCollection<LiteDBPlayerInfo> SteamCollection { get; private set; }
        public ILiteCollection<LiteDBPlayerInfo> DiscordCollection { get; private set; }
        public ILiteCollection<LiteDBPlayerInfo> NWCollection { get; private set; }

        private LiteDatabase database;

        public override void Initialize()
        {
            database = new LiteDatabase(Config.File);

            SteamCollection = database.GetCollection<LiteDBPlayerInfo>("playerinfo_steam");
            SteamCollection.EnsureIndex(x => x.Id);

            DiscordCollection = database.GetCollection<LiteDBPlayerInfo>("playerinfo_discord");
            DiscordCollection.EnsureIndex(x => x.Id);

            NWCollection = database.GetCollection<LiteDBPlayerInfo>("playerinfo_nw");
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

            database.Dispose();
            database = null;
        }
        
        public ILiteCollection<LiteDBPlayerInfo> GetCollection(PlayerId playerId) => playerId.AuthType switch
        {
            AuthType.Steam => SteamCollection,
            AuthType.Discord => DiscordCollection,
            AuthType.Northwood => NWCollection,
            _ => throw new ArgumentOutOfRangeException(nameof(playerId.AuthType), playerId.AuthType, null)
        };

        protected override bool TryGetPlayerInfoNoCache(PlayerId playerId, out PlayerInfo playerInfo)
        {
            var collection = GetCollection(playerId);
            var existing = collection.FindOne(x => x.Id == playerId.Id); // See note on SetPlayerInfoNoCache

            if (existing == null)
            {
                playerInfo = null;
                return false;
            }

            playerInfo = existing.ToPlayerInfo(playerId.AuthType);
            return true;
        }

        protected override PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache(PlayerId playerId)
        {
            var collection = GetCollection(playerId);
            var existing = collection.FindOne(x => x.Id == playerId.Id); // See note on SetPlayerInfoNoCache
            
            if (existing == null)
            {
                existing = new LiteDBPlayerInfo()
                {
                    Id = playerId.Id,
                    XP = 0
                };

                collection.Insert(existing);
            }

            return existing.ToPlayerInfo(playerId.AuthType);
        }

        public override IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count)
        {
            var result = SteamCollection.Query()
                .OrderByDescending(x => x.XP)
                .Limit(count)
                .ToEnumerable()
                .Select(x => x.ToPlayerInfo(AuthType.Steam))
                .ToList();

            result.AddRange(DiscordCollection.Query()
                .OrderByDescending(x => x.XP)
                .Limit(count)
                .ToEnumerable()
                .Select(x => x.ToPlayerInfo(AuthType.Discord)));

            result.AddRange(NWCollection.Query()
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
            var collection = GetCollection(playerInfo.Player);
            var existing = collection.FindOne(x => x.Id == playerInfo.Player.Id); // FindById no work; BsonValue converts ulong to decimal, key is different; retarded conversion

            if (existing == null)
            {
                collection.Insert(new LiteDBPlayerInfo()
                {
                    Id = playerInfo.Player.Id,
                    XP = playerInfo.XP,
#if STORENICKS
                    Nickname = playerInfo.Nickname
#endif
                });
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

        protected override bool DeletePlayerInfoNoCache(PlayerId playerId)
        {
            var collection = GetCollection(playerId);
            return collection.Delete(playerId.Id);
        }

        protected override void DeleteAllPlayerInfoNoCache()
        {
            SteamCollection.DeleteAll();
            DiscordCollection.DeleteAll();
            NWCollection.DeleteAll();
        }

        public class LiteDBProviderConfig
        {
            public bool IndexDB { get; set; } = true;
            public string File { get; set; } = "database.db";
        }
    }
}