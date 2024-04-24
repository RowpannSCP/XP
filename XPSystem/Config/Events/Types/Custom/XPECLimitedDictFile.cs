namespace XPSystem.Config.Events.Types.Custom
{
    using System.ComponentModel;

    /// <summary>
    /// XP Event Config limited dictionary file.
    /// Events that can only be achieved a limited number of times.
    /// </summary>
    /// <typeparam name="T">The type of the subkeys.</typeparam>
    public class XPECLimitedDictFile<T> : XPECDictFile<T>
    {
        [Description("Whether or not to always allow gaining xp. If enabled, will ignore all other limit settings.")]
        public bool AlwaysAllow { get; set; } = false;

        [Description("Whether or not a a event with Amount set to 0 will affect/trigger the limit.")]
        public bool ZeroXPGainTriggers { get; set; } = false;

        [Description("Whether or not the limit is per-file (ex. for all pickups) or per-subkey (ex. for each pickup type)")]
        public bool LimitUnified { get; set; } = true;

        [Description("The limit of how many times the event can be achieved per round. Negative for no limit.")]
        public int RoundLimit { get; set; } = -1;

        [Description("The limit of how many times the event can be achieved per life (until role change). Negative for no limit.")]
        public int LifeLimit { get; set; } = -1;

        [Description("The minimum amount of time needed to pass before the event can be achieved again. Negative for no cooldown.")]
        public float Cooldown { get; set; } = -1f;
    }
}