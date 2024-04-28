namespace XPSystem.API
{
    using System;
    using XPSystem.API.StorageProviders;

    /// <summary>
    /// <see cref="XPDisplayProvider{T}"/>, just without generic.
    /// </summary>
    public interface IXPDisplayProvider
    {
        void Enable();
        void Disable();
        void Refresh(XPPlayer player, PlayerInfoWrapper playerInfo);
        void RefreshAll();
        IXPDisplayProviderConfig ConfigPropertyInternal { get; set; }
        Type ConfigTypeInternal { get; }
    }
}