namespace XPSystem.LiteDBProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LiteDB;
    using XPSystem.API.Enums;
    using XPSystem.API.Legacy;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.LiteDBProvider.Models;
    using static API.LoaderSpecific;

    public class LiteDBProvider : StorageProvider<LiteDBProvider.LiteDBProviderConfig>
    {
        public ILiteCollection<LiteDBPlayerInfo> SteamCollection { get; private set; }
        public ILiteCollection<LiteDBPlayerInfo> DiscordCollection { get; private set; }
        public ILiteCollection<LiteDBPlayerInfo> NWCollection { get; private set; }

        private LiteDatabase database;

        protected override void InitializeNoCache()
        {
            database = new LiteDatabase(Config.File);

            if (LiteDBMigrator.CanMigrate(database))
            {
                for (int i = 0; i < 3; i++) LogInfo("Found legacy data in database, migrate using xps migrate in server console!.");
            }

            SteamCollection = database.GetCollection<LiteDBPlayerInfo>("playerinfo-steam");
            SteamCollection.EnsureIndex(x => x.Id);

            DiscordCollection = database.GetCollection<LiteDBPlayerInfo>("playerinfo-discord");
            DiscordCollection.EnsureIndex(x => x.Id);

            NWCollection = database.GetCollection<LiteDBPlayerInfo>("playerinfo-nw");
            NWCollection.EnsureIndex(x => x.Id);

            if (Config.IndexDB)
            {
                SteamCollection.EnsureIndex(x => x.XP);
                DiscordCollection.EnsureIndex(x => x.XP);
                NWCollection.EnsureIndex(x => x.XP);
            }
        }

        protected override void DisposeNoCache()
        {
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

        protected override bool TryGetPlayerInfoNoCache(PlayerId playerId, out PlayerInfo liteDBPlayerInfo)
        {
            var collection = GetCollection(playerId);
            var result = collection.FindById(playerId.Id);
            if (result == null)
            {
                liteDBPlayerInfo = null;
                return false;
            }

            liteDBPlayerInfo = result.ToPlayerInfo(playerId.AuthType);
            return true;
        }

        protected override PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache(PlayerId playerId)
        {
            var collection = GetCollection(playerId);
            var result = collection.FindById(playerId.Id);
            
            if (result == null)
            {
                result = new LiteDBPlayerInfo()
                {
                    Id = playerId.Id,
                    XP = 0
                };

                collection.Insert(result);
            }

            return result.ToPlayerInfo(playerId.AuthType);
        }

        public override IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count)
        {
            var result = SteamCollection.Query()
                .OrderByDescending(x => x.XP)
                .Limit(count)
                .ToEnumerable()
                .Select(x => x.ToPlayerInfo(AuthType.Steam))
                .ToList();

            var discord = DiscordCollection.Query()
                .OrderByDescending(x => x.XP)
                .Limit(count)
                .ToEnumerable()
                .Select(x => x.ToPlayerInfo(AuthType.Discord));

            var nw = NWCollection.Query()
                .OrderByDescending(x => x.XP)
                .Limit(count)
                .ToEnumerable()
                .Select(x => x.ToPlayerInfo(AuthType.Northwood));

            result.AddRange(discord);
            result.AddRange(nw);

            result.Sort((x, y) => y.XP.CompareTo(x.XP));

            return result.Take(count)
                .Select(x => new PlayerInfoWrapper(x));
        }

        protected override void SetPlayerInfoNoCache(PlayerInfo liteDBPlayerInfo)
        {
            var collection = GetCollection(liteDBPlayerInfo.Player);
            var result = collection.FindById(liteDBPlayerInfo.Player.Id);

            if (result == null)
            {
                collection.Insert(new LiteDBPlayerInfo()
                {
                    Id = liteDBPlayerInfo.Player.Id,
                    XP = liteDBPlayerInfo.XP
                });
            }
            else
            {
                result.XP = liteDBPlayerInfo.XP;
                collection.Update(result);
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