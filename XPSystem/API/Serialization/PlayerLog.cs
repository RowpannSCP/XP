using System;
using LiteDB;

namespace XPSystem.API.Serialization
{
    [Serializable]
    public class PlayerLog
    {
        public int LVL { get; set; }
        public int XP { get; set; }
        [BsonId]
        public string ID { get; set; }
    }
}