namespace XPSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using HarmonyLib;
    using XPSystem.API;
    using XPSystem.API.Legacy;
    using XPSystem.API.StorageProviders;
    using XPSystem.Config;
    using XPSystem.Config.Events;
    using XPSystem.EventHandlers;
    using XPSystem.EventHandlers.LoaderSpecific;
    using XPSystem.XPDisplayProviders;
    using static API.XPAPI;

    public class Main
#if EXILED
        : Exiled.API.Features.Plugin<ExiledConfig>
#endif
    {
        public const string VersionString = "2.0.1";

        /// <summary>
        /// This number is increased every time the plugin is reloaded.
        /// Store last calculated value to check if recalculation is needed.
        /// </summary>
        public static int Reload = 0;

#if EXILED
        private static readonly int[] _splitVersion = VersionString
            .Split('.')
            .Select(x => Convert.ToInt32(x))
            .ToArray();

        public override string Author { get; } = "moddedmcplayer, original by BrutoForceMaestro";
        public override string Name { get; } = "XPSystem";
        public override Version Version { get; } = new Version(_splitVersion[0], _splitVersion[1], _splitVersion[2]);
        public override Version RequiredExiledVersion { get; } = new Version(8, 0, 0);
#else
        [PluginAPI.Core.Attributes.PluginConfig]
        public NwAPIConfig Config;
#endif

        public static Main Instance { get; private set; }
        public Harmony Harmony { get; private set; }

        private UnifiedEventHandlers _eventHandlers = new
#if EXILED
            ExiledEventHandlers();
#else
            NWAPIEventHandlers();
#endif

#if EXILED
        public override void OnEnabled()
#else
        [PluginAPI.Core.Attributes.PluginEntryPoint("xpsystem", VersionString, "xp plugin", "Rowpann's Emperium, original by BrutoForceMaestro")]
        public void OnEnabled()
#endif
        {
            Instance = this;
            XPAPI.Config = Config;
            Harmony = new Harmony($"XPSystem - {DateTime.Now.Ticks}");
            Harmony.PatchAll();

            DisplayProviders.Add(new NickXPDisplayProvider());
            DisplayProviders.Add(new RankXPDisplayProvider());
            MessagingProvider = MessagingProviders.Get(Config.DisplayMode);
            XPECLimitTracker.Initialize();

            LoadExtraConfigs();

            _eventHandlers.RegisterEvents(this);
            PluginEnabled = true;

            LiteDBMigrator.CheckMigration();

#if STORENICKS
            LogInfo("STORENICKS");
#endif
 
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

            SetStorageProvider((IStorageProvider)null);
            MessagingProvider = null;

            DisplayProviders.DisableAll();
            XPECLimitTracker.Disable();

            XPECManager.Default.Files.Clear();
            XPECManager.Overrides.Clear();

            Harmony.UnpatchAll(Harmony.Id);
            Harmony = null;
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
            LoadExtraConfigs();
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
                Directory.CreateDirectory(Config.ExtendedConfigPath);

                Reload++;

                SetStorageProvider(Config.StorageProvider);
                SetDisplayProviders(Config.AdditionalDisplayProviders);

                DisplayProviders.LoadConfigs(Config.ExtendedConfigPath);

                string eventConfigsFolder = Path.Combine(Config.ExtendedConfigPath, Config.EventConfigsFolder);
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