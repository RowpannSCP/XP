namespace XPSystem.Config.Events.Types.Custom
{
    /// <summary>
    /// Represents a limit configuration.
    /// In other words, <see cref="XPECLimitedDictFile{T}"/> just non-generic.
    /// </summary>
    public interface IXPECLimitedFile
    {
        public bool AlwaysAllow { get; set; }
        public bool ZeroXPGainTriggers { get; set; }
        public bool LimitUnified { get; set; }
        public int RoundLimit { get; set; }
        public int LifeLimit { get; set; }
        public float Cooldown { get; set; }
    }
}