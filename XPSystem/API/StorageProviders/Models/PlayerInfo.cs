namespace XPSystem.API.StorageProviders.Models
{
    /// <summary>
    /// Contains all database data for a player.
    /// You shouldn't use this directly with your database library, instead convert from and from this class instead.
    /// </summary>
    public class PlayerInfo
    {
        public PlayerId Player { get; set; }
        public int XP { get; set; }
#if STORENICKS
        public string Nickname { get; set; }
#endif
    }
}