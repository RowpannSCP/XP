using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using Utf8Json;

namespace XPSystem
{
    class JsonSerialization
    {
        static string SP = Main.Instance.Config.SavePath;
        public static void Save()
        {
            var PlayerLogDict = new Dictionary<string, PlayerLogSerializable>();
            foreach (var playerLogInternal in Main.Players)
            {
                PlayerLogDict[playerLogInternal.Key] = playerLogInternal.Value;
            }
            using (FileStream fs = File.Create(SP))
            {
                JsonSerializer.Serialize(fs, Main.Players);
            }
        }
        public static void Read()
        {
            string text = File.ReadAllText(SP);
            try
            {
                var PlayerLogDict = JsonSerializer.Deserialize<Dictionary<string, PlayerLogSerializable>>(text);
                foreach (var playerLog in PlayerLogDict)
                {
                    Main.Players[playerLog.Key] = new PlayerLog(playerLog.Value);
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Log.Debug("Json file is empty", Main.Instance.Config.ShowDebug);
            }
        }
    }
}
