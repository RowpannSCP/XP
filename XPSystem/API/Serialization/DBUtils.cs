using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using LiteDB;
using JsonSerializer = Utf8Json.JsonSerializer;

namespace XPSystem.API.Serialization
{
    public class DBUtils
    {
        static string SP = Main.Instance.Config.SavePath;
        public static void Save()
        {
            var PlayerLogDict = new Dictionary<string, PlayerLogSerializable>();
            foreach (var playerLogInternal in Main.Players)
            {
                PlayerLogDict[playerLogInternal.Key] = playerLogInternal.Value;
            }
            using (LiteDatabase db = new LiteDatabase(SP))
            {
                var col = db.GetCollection<KeyValuePair<string, PlayerLog>>("players");
                col.InsertBulk(Main.Players);
                col.Update(Main.Players);
            }
        }
        public static void Read()
        {
            using (LiteDatabase db = new LiteDatabase(SP))
            {
                var col = db.GetCollection<KeyValuePair<string, PlayerLog>>("players");
                col.InsertBulk(Main.Players);
                col.Update(Main.Players);
                try
                {
                    var PlayerLogDict = db.GetCollection<KeyValuePair<string, PlayerLog>>("players").FindAll();
                    foreach (var playerLog in PlayerLogDict)
                    {
                        Main.Players[playerLog.Key] = new PlayerLog(playerLog.Value);
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    Log.Debug("Database is empty", Main.Instance.Config.ShowDebug);
                }
            }
        }
    }
}