namespace XPSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using HarmonyLib;
    using XPSystem.API;
    using XPSystem.API.StorageProviders;
    using XPSystem.Config;
    using XPSystem.Config.Events;
    using XPSystem.EventHandlers;
    using XPSystem.EventHandlers.LoaderSpecific;
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

#error more debug

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

            DisplayProviders.Add(new NickXPDisplayProvider());
            DisplayProviders.Add(new RankXPDisplayProvider());
            MessagingProvider = MessagingProviders.Get(Config.DisplayMode);

            LoadExtraConfigs();

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

            DisplayProviders.DisableAll();
            MessagingProvider = null;
            XPECManager.Default.Files.Clear();
            XPECManager.Overrides.Clear();

            _harmony.UnpatchAll(_harmony.Id);
            _harmony = null;
            Instance = null;
#if EXILED
            base.OnDisabled();
#endif
        }

#if EXILED
        public override void OnReloaded()
#else
        [PluginAPI.Core.Attributes.PluginReload]
        public void OnReloaded()
#endif
        {
            LevelCalculator.Precalculate();
        }

        public void SetDisplayProviders(IEnumerable<string> typeNames)
        {
            foreach (var typeName in typeNames)
            {
                if (!TryCreate(typeName, out var exception, out IXPDisplayProvider provider))
                {
                    LogError($"Could not create display provider {typeName}: {exception}");
                    continue;
                }

                DisplayProviders.Add(provider);
            }
        }

        public void LoadExtraConfigs()
        {
            try
            {
                SetStorageProvider(Config.StorageProvider);
                SetDisplayProviders(Config.AdditionalDisplayProviders);

                if (!Directory.Exists(Config.ExtendedConfigPath))
                    Directory.CreateDirectory(Config.ExtendedConfigPath);

                DisplayProviders.LoadConfigs(Config.ExtendedConfigPath);

                var eventConfigsFolder = Path.Combine(Config.ExtendedConfigPath, Config.EventConfigsFolder);
                Directory.CreateDirectory(eventConfigsFolder);
                XPECManager.Load(eventConfigsFolder);

                LevelCalculator.Precalculate();
                DisplayProviders.Enable();
            }
            catch (Exception e)
            {
                LogError("Could not load extra configs: " + e);
            }
        }

        public static bool TryCreate<T>(string typeName, out Exception exception, out T obj)
        {
            obj = default;
            exception = null;

            try
            {
                var type = Type.GetType(typeName) ?? throw new TypeLoadException("Type not found!");
                obj = (T)Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }

            return true;
        }
    }
}