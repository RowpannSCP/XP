namespace XPSystem.API.Legacy
{
    using System;
    using System.IO;
    using LiteDB;
    using XPSystem.API.StorageProviders.Models;
    using static XPAPI;

    public static class LiteDBMigrator
    {
        public static void CheckMigration()
        {
            string path = Config.LegacyDefaultDatabasePath;
            LogDebug("Checking for legacy database at: " + path);

            if (!File.Exists(path))
                return;

            try
            {
                using var database = new LiteDatabase(path);
                int count = CanMigrate(database);

                if (count > 0)
                {
                    for (int i = 0; i < 3; i++)
                        LogInfo($"Found legacy data ({count} entries) in old database, migrate using xps migrate in server console!.");
                }
            }
            catch (Exception e)
            {
                LogDebug($"Error while checking for legacy database: {e}");
            }
        }

        /// <summary>
        /// Checks if the database can be migrated.
        /// </summary>
        /// <param name="db">The lite database to check.</param>
        /// <returns>The amount of entries that can be migrated.</returns>
        public static int CanMigrate(LiteDatabase db)
        {
            if (!db.CollectionExists("Players"))
                return -1;

            return db.GetCollection<PlayerLog>("Players").Count();
        }

        /// <summary>
        /// Imports a legacy database to the current <see cref="XPSystem.API.StorageProviders.IStorageProvider"/>.
        /// </summary>
        /// <param name="db">The legacy database to import.</param>
        /// <returns>The amount of players imported.</returns>
        public static int ImportLegacyDB(LiteDatabase db)
        {
            EnsureStorageProviderValid();
            if (!db.CollectionExists("Players"))
                return 0;

            var collection = db.GetCollection<PlayerLog>("Players");
            int count = 0;
            int total = collection.Count();

            foreach (PlayerLog log in collection.FindAll())
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

                StorageProvider.SetPlayerInfo(playerInfo);
                count++;

                if (count % 100 == 0) LogInfo($"Imported {count}/{total} players.");
            }

            LogInfo($"Finished importing {count}/{total} players.");
            db.RenameCollection("Players", "Players_legacy");
            LogInfo("Renamed Players to Players_legacy.");

            return count;
        }
    }
}