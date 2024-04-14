namespace XPSystem.API
{
    using System;

    /// <summary>
    /// <see cref="XPDisplayProvider{T}"/>, just without generic.
    /// </summary>
    public interface IXPDisplayProvider
    {
        void Enable();
        void Disable();
        void Refresh(XPPlayer player);
        void RefreshAll();
        IXPDisplayProviderConfig ConfigPropertyInternal { get; set; }
        Type ConfigTypeInternal { get; }
    }
}