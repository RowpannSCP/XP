namespace XPSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using HarmonyLib;
    using XPSystem.API;
    using XPSystem.API.Legacy;
    using XPSystem.API.StorageProviders;
    using XPSystem.BuiltInProviders.Display.Patch;
    using XPSystem.Commands.Client;
    using XPSystem.Config;
    using XPSystem.Config.Events;
    using XPSystem.EventHandlers;
    using XPSystem.EventHandlers.LoaderSpecific;
    using XPSystem.XPDisplayProviders;
    using static API.XPAPI;

    public class Main
#if EXILED
        : Exiled.API.Features.Plugin<ExiledConfig>
#else
        : LabApi.Loader.Features.Plugins.Plugin<NwAPIConfig>
#endif
    {
        public const string VersionString = "2.1.0";

        /// <summary>
        /// This number is increased every time the plugin is reloaded.
        /// Store last calculated value to check if recalculation is needed.
        /// </summary>
        public static int Reload = 0;

        /// <summary>
        /// Invoked when <see cref="Reload"/> is incremented.
        /// </summary>
        public static event Action Reloaded = delegate { };

        private static readonly int[] _splitVersion = VersionString
            .Split('.')
            .Select(x => Convert.ToInt32(x))
            .ToArray();
        public override Version Version { get; } = new Version(_splitVersion[0], _splitVersion[1], _splitVersion[2]);
        public override string Author { get; } = "moddedmcplayer, original by BrutoForceMaestro";
        public override string Name { get; } = "XPSystem";

#if EXILED
        
        public override Version RequiredExiledVersion { get; } = Exiled.Loader.Loader.Version;
#else
        public override string Description { get; } = "A not so basic, customisable leveling system for SCP: SL.";
        public override Version RequiredApiVersion { get; } = LabApi.Features.LabApiProperties.CurrentVersion;
#endif

        public static Main? Instance { get; private set; }
        public Harmony? Harmony { get; private set; }

        private readonly UnifiedEventHandlers _eventHandlers = new
#if EXILED
            ExiledEventHandlers();
#else
            NWAPIEventHandlers();
#endif

#if EXILED
        public override void OnEnabled()
#else
        
        public override void Enable()
#endif
        {
#if !EXILED
            Config = Config ?? Config!; // why nullable (labapi ragebait)
#endif

            Instance = this;
            XPAPI.Config = Config;
            Harmony = new Harmony($"XPSystem - {DateTime.Now.Ticks}");
            Harmony.PatchAll();

            DisplayProviders.Add(new NickPatchXPDisplayProvider());
            DisplayProviders.Add(new RankXPDisplayProvider());
            MessagingProvider = MessagingProviders.Get(Config.DisplayMode);
            XPECLimitTracker.Initialize();

            LoadExtraConfigs();

            _eventHandlers.RegisterEvents(this);
            PluginEnabled = true;

            LiteDBMigrator.CheckMigration();
            ClientAliasManager.RegisterAliases();

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
        public override void Disable()
#endif
        {
            PluginEnabled = false;
            _eventHandlers.UnregisterEvents(this);

            DisplayProviders.DisableAll();
            XPECLimitTracker.Disable();
            ClientAliasManager.UnregisterAliases();

            SetStorageProvider((IStorageProvider?)null);
            MessagingProvider = null;

            XPECManager.Default.Files.Clear();
            XPECManager.Overrides.Clear();

            Harmony?.UnpatchAll(Harmony.Id);
            Harmony = null;
            Instance = null;
#if EXILED
            base.OnDisabled();
#endif
        }

#if EXILED
        public override void OnReloaded()
#else
        public void OnReloaded()
#endif
        {
            LoadExtraConfigs();
        }

        public void SetDisplayProviders(IEnumerable<string> typeNames)
        {
            foreach (string typeName in typeNames)
            {
                if (!TryCreate(typeName, out Exception? exception, out IXPDisplayProvider? provider))
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
                Directory.CreateDirectory(Config!.ExtendedConfigPath);

                SetStorageProvider(Config.StorageProvider);
                SetDisplayProviders(Config.AdditionalDisplayProviders);

                DisplayProviders.LoadConfigs(Config.ExtendedConfigPath);

                string eventConfigsFolder = Path.Combine(Config.ExtendedConfigPath, Config.EventConfigsFolder);
                Directory.CreateDirectory(eventConfigsFolder);
                XPECManager.Load(eventConfigsFolder);

                LevelCalculator.Init();
                DisplayProviders.Enable();

                Reload++;
                Reloaded.Invoke();
            }
            catch (Exception e)
            {
                LogError("Could not load extra configs: " + e);
            }
        }

        public static bool TryCreate<T>(string typeName, [NotNullWhen(false)] out Exception? exception, [NotNullWhen(true)] out T? obj)
        {
            obj = default;
            exception = null;

            try
            {
                Type type = Type.GetType(typeName) ?? throw new TypeLoadException("Type not found!");
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