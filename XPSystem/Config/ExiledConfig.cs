#if EXILED

namespace XPSystem.Config
{
    using Exiled.API.Interfaces;

    public class ExiledConfig : ConfigShared, IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }
}

#endif