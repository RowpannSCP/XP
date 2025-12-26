namespace XPSystem.API
{
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;

    /// <summary>
    /// <see cref="XPDisplayProvider{T}"/>, just without generic.
    /// </summary>
    public interface IXPDisplayProvider
    {
        void Enable();
        void Disable();
        void RefreshTo(BaseXPPlayer player);
        void RefreshOf(BaseXPPlayer player, PlayerInfoWrapper? playerInfo = null);
        void RefreshAll();
    }
}