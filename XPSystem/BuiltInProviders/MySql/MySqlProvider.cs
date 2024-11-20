namespace XPSystem.BuiltInProviders.MySql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MySqlConnector;
    using XPSystem.API;
    using XPSystem.API.Enums;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    public class MySqlProvider : StorageProvider<MySqlProvider.MySqlProviderConfig>
    {
        public const string TableBaseName = "playerinfo";
        public string GetTableName(AuthType authType) => $"{TableBaseName}_{authType.ToString()}";

        public override void Initialize()
        {
            using var connection = GetConnection();

            foreach (AuthType authType in Enum.GetValues(typeof(AuthType)))
            {
                using var command = connection.CreateCommand();
                command.CommandText =
                    $"CREATE TABLE IF NOT EXISTS {GetTableName(authType)} (" +
                    (authType == AuthType.Northwood ? "id VARCHAR(32) PRIMARY KEY" : "id BIGINT UNSIGNED PRIMARY KEY,") +
                    "xp int UNSIGNED NOT NULL DEFAULT 0" +
#if STORENICKS
                    ",nickname VARCHAR(64)" +
#endif
                    ")";

                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        public override IEnumerable<PlayerInfoWrapper> GetTopPlayers(int count)
        {
            using var connection = GetConnection();

            List<PlayerInfo> topPlayers = new();

            foreach (AuthType authType in Enum.GetValues(typeof(AuthType)))
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM {GetTableName(authType)} ORDER BY xp DESC LIMIT {count}";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    topPlayers.Add(FromReader(reader, null, authType));
                }
            }

            topPlayers.Sort((x, y) => y.XP.CompareTo(x.XP));

            return topPlayers.Take(count)
                .Select(x => new PlayerInfoWrapper(x));
        }

        protected override bool TryGetPlayerInfoNoCache(IPlayerId<object> playerId, out PlayerInfo playerInfo)
        {
            using var connection = GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT * FROM {GetTableName(playerId.AuthType)} WHERE id = {playerId.Id}";

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                playerInfo = null;
                return false;
            }

            playerInfo = FromReader(reader, playerId);
            return true;
        }

        protected override PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache(IPlayerId<object> playerId)
        {
            using var connection = GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT * FROM {GetTableName(playerId.AuthType)} WHERE id = {playerId.Id}";

            using var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                using var insertConnection = GetConnection();
                using var insertCommand = insertConnection.CreateCommand();
                insertCommand.CommandText =
                    $"INSERT INTO {GetTableName(playerId.AuthType)} (id, xp) VALUES ({playerId.Id}, 0)";
                insertCommand.ExecuteNonQuery();

                return new PlayerInfo
                {
                    Player = playerId,
                    XP = 0
                };
            }

            reader.Read();
            return FromReader(reader, playerId);
        }

        protected override void SetPlayerInfoNoCache(PlayerInfo playerInfo)
        {
            using var connection = GetConnection();

            using var command = connection.CreateCommand();
            command.CommandText =
                $"REPLACE INTO {GetTableName(playerInfo.Player.AuthType)} (id, xp" +
#if STORENICKS
                ",nickname" +
#endif
                $") VALUES ({playerInfo.Player.Id}, {playerInfo.XP}" +
#if STORENICKS
                ",@nickname" +
#endif
                ")";
#if STORENICKS
            command.Parameters.AddWithValue("@nickname", playerInfo.Nickname ?? "NULL");
#endif

            command.ExecuteNonQuery();
        }

        protected override bool DeletePlayerInfoNoCache(IPlayerId<object> playerId)
        {
            using var connection = GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"DELETE FROM {GetTableName(playerId.AuthType)} WHERE id = {playerId.GetId()}";

            return command.ExecuteNonQuery() > 0;
        }

        protected override void DeleteAllPlayerInfoNoCache()
        {
            using var connection = GetConnection();

            foreach (AuthType authType in Enum.GetValues(typeof(AuthType)))
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"DELETE FROM {GetTableName(authType)}";
                command.ExecuteNonQuery();
            }
        }

        private PlayerInfo FromReader(MySqlDataReader reader, IPlayerId<object> playerId = null, AuthType? authType = null)
        {
            if (playerId == null)
            {
                switch (authType)
                {
                    case AuthType.Steam:
                        playerId = new NumberPlayerId(reader.GetUInt64(0), AuthType.Steam);
                        break;
                    case AuthType.Discord:
                        playerId = new NumberPlayerId(reader.GetUInt64(0), AuthType.Discord);
                        break;
                    case AuthType.Northwood:
                        playerId = new StringPlayerId(reader.GetString(0), AuthType.Northwood);
                        break;
                    case null:
                        throw new ArgumentNullException(nameof(authType));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(authType), authType, null);
                }
            }

            return new PlayerInfo
            {
                Player = playerId,
                XP = reader.GetInt32(1),
#if STORENICKS
                Nickname = reader.IsDBNull(2) ? null : reader.GetString(2)
#endif
            };
        }

        private void Log(object sender, MySqlInfoMessageEventArgs ev) => XPAPI.LogDebug(ev.ToString());

        public MySqlConnection GetConnection()
        {
            MySqlConnection conn = new(Config.ConnectionString);

            if (Config.LogQueries)
                conn.InfoMessage += Log;

            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                conn.Dispose();
                throw new Exception("Failed to open connection to database.", e);
            }

            return conn;
        }

        public class MySqlProviderConfig
        {
            public string ConnectionString { get; set; }
            public bool LogQueries { get; set; } = false;
        }
    }
}