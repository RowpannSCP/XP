namespace XPSystem
{
    using System;
    using System.IO;
    using System.Linq;
    using HarmonyLib;
    using XPSystem.API;
    using XPSystem.API.Config;
    using XPSystem.Config;
    using XPSystem.EventHandlers;
    using XPSystem.EventHandlers.LoaderSpecific;
    using XPSystem.Patches;
    using XPSystem.XPDisplayProviders;
    using static API.LoaderSpecific;
    using static API.XPAPI;

    public class Main
#if EXILED
        : Exiled.API.Features.Plugin<ExiledConfig>
#endif
    {
        public const string VersionString = "2.0.0";

#if EXILED
        private static readonly int[] _splitVersion = VersionString.Split('.').Select(x => Convert.ToInt32(x)).ToArray();
        public override string Author { get; } = "Rowpann's Emperium, original by BrutoForceMaestro";
        public override string Name { get; } = "XPSystem";
        public override Version Version { get; } = new Version(_splitVersion[0], _splitVersion[1], _splitVersion[2]);
        public override Version RequiredExiledVersion { get; } = new Version(8, 0, 0);
#else
        [PluginAPI.Core.Attributes.PluginConfig]
        public NwAPIConfig Config;
#endif

        public static Main Instance { get; set; }
        private UnifiedEventHandlers _eventHandlers = new
#if EXILED
            ExiledEventHandlers();
#else
            NWAPIEventHandlers();
#endif
        private Harmony _harmony;

#if EXILED
        public override void OnEnabled()
#else
        [PluginAPI.Core.Attributes.PluginEntryPoint("xpsystem", VersionString, "xp plugin", "Rowpann's Emperium, original by BrutoForceMaestro")]
        public void OnEnabled()
#endif
        {
            Instance = this;
            XPAPI.Config = Config;
            _harmony = new Harmony($"XPSystem - {DateTime.Now.Ticks}");
            _harmony.PatchAll();

            LoadExtraConfigs();

            if (Config.EnableNicks)
                DisplayProviders.Add(new NickXPDisplayProvider());
            if (Config.EnableBadges)
                DisplayProviders.Add(new RankXPDisplayProvider());

            MessagingProvider = MessagingProviders.Get(Config.DisplayMode);

            _eventHandlers.RegisterEvents(this);

            PluginEnabled = true;
#if EXILED
            base.OnEnabled();
#endif
        }

#if EXILED
        public override void OnDisabled()
#else
        [PluginAPI.Core.Attributes.PluginUnload]
        public void OnDisabled()
#endif
        {
            PluginEnabled = false;

            _eventHandlers.UnregisterEvents(this);

            DisplayProviders.RemoveAll();
            MessagingProvider = null;
            XPConfigManager.XPConfigs.Clear();

            _harmony.UnpatchAll(_harmony.Id);

            _harmony = null;
            Instance = null;
#if EXILED
            base.OnDisabled();
#endif
        }

        private void LoadExtraConfigs()
        {
            try
            {
                if (!Directory.Exists(Config.ExtendedConfigPath))
                    Directory.CreateDirectory(Config.ExtendedConfigPath);

                var file = Path.Combine(Config.ExtendedConfigPath, Config.XPConfigsFile);
                using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    XPConfigManager.LoadConfigs(fs);
                }
            }
            catch (Exception e)
            {
                LogError("Could not load extra configs: " + e);
            }
        }
    }
}