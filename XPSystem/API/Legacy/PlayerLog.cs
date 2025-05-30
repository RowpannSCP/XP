namespace XPSystem.API.Legacy
{
    using System;
    using LiteDB;

    /// <summary>
    /// Legacy class, used for the old database system.
    /// </summary>
    [Serializable]
    public class PlayerLog
    {
        public int LVL { get; set; }
        public int XP { get; set; }
        [BsonId]
        public string ID { get; set; } = null!;
    }
}