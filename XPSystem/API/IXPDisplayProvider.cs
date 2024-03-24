namespace XPSystem.API
{
    /// <summary>
    /// Interface for xp display provider, shows the xp/lvl of a player to others.
    /// </summary>
    public interface IXPDisplayProvider
    {
        void Enable();
        void Disable();
        void Refresh(XPPlayer player);
        void RefreshAll();
    }
}