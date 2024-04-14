namespace XPSystem.API.Legacy
{
    using LiteDB;
    using XPSystem.API.StorageProviders.Models;
    using static LoaderSpecific;

    public static class LiteDBMigrator
    {
        /// <summary>
        /// Gets whether or not the database has legacy data that can be migrated.
        /// </summary>
        /// <param name="db">The database to check.</param>
        public static bool CanMigrate(LiteDatabase db)
        {
            return db.CollectionExists("Players")
                   && db.GetCollection<PlayerLog>("Players").Count() > 0;
        }

        /// <summary>
        /// Imports a legacy database to the current <see cref="XPSystem.API.StorageProviders.IStorageProvider"/>.
        /// </summary>
        /// <param name="db">The legacy database to import.</param>
        /// <returns>The amount of players imported.</returns>
        public static int ImportLegacyDB(LiteDatabase db)
        {
            XPAPI.EnsureStorageProviderValid();
            if (!db.CollectionExists("Players"))
                return 0;

            var collection = db.GetCollection<PlayerLog>("Players");
            int count = 0;
            int total = collection.Count();

            foreach (var log in collection.FindAll())
            {
                if (!log.ID.TryParseUserId(out var id))
                {
                    LogWarn(
                        $"(ImportLegacyDB) Skipping {log.ID} (LVL: {log.LVL}, XP: {log.XP}) because id could not be parsed.");
                    continue;
                }

                var playerInfo = new PlayerInfo
                {
                    Player = id,
                    XP = log.XP + LevelCalculator.GetXP(log.LVL)
                };

                XPAPI.StorageProvider.SetPlayerInfo(playerInfo);
                count++;

                if (count % 100 == 0) LogInfo($"Imported {count}/{total} players.");
            }

            LogInfo($"Finished importing {count}/{total} players.");
            db.RenameCollection("Players", "Players-legacy");
            LogInfo("Renamed Players to Players-legacy.");

            return count;
        }
    }
}