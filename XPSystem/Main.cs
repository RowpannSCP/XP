namespace XPSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using HarmonyLib;
    using LiteDB;
    using MEC;
    using XPSystem.API;
    using YamlDotNet.Serialization;

    public class Main
#if EXILED
        : Exiled.API.Features.Plugin<Config>
#endif
    {
        private const string _version = "1.8.4";
        public static Main Instance { get; set; }
        public EventHandlers handlers;
        private Harmony _harmony;
        public LiteDatabase db;

        public Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            ["ExampleKey"] = "ExampleValue",
            ["ExampleKey2"] = "ExampleValue",
        };

#if EXILED
        private static int[] split = _version.Split('.').Select(x => Convert.ToInt32(x)).ToArray();
        public override string Author { get; } = "Rowpann's Emperium, original by BrutoForceMaestro";
        public override string Name { get; } = "XPSystem";
        public override Version Version { get; } = new Version(split[0], split[1], split[2]);
        public override Version RequiredExiledVersion { get; } = new Version(7, 0, 0);
#else
        [PluginAPI.Core.Attributes.PluginConfig]
        public Config Config;
#endif

#if EXILED
        public override void OnEnabled()
#else
        [PluginAPI.Core.Attributes.PluginEntryPoint("xpsystem", _version, "xp plugin", "Rowpann's Emperium, original by BrutoForceMaestro")]
        public void OnEnabled()
#endif
        {
            db = new LiteDatabase(Config.SavePath);
            Instance = this;
            _harmony = new Harmony($"XPSystem - {DateTime.Now.Ticks}");
            _harmony.PatchAll();

#if EXILED
            handlers = new EventHandlers();
            Exiled.Events.Handlers.Player.Verified += handlers.OnJoined;
            Exiled.Events.Handlers.Player.Died += handlers.OnKill;
            Exiled.Events.Handlers.Server.RoundEnded += handlers.OnRoundEnd;
            Exiled.Events.Handlers.Player.Escaping += handlers.OnEscape;
            Exiled.Events.Handlers.Player.InteractingDoor += handlers.OnInteractingDoor;
            Exiled.Events.Handlers.Scp914.UpgradingPickup += handlers.OnScp914UpgradingItem;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem += handlers.OnScp914UpgradingInventory;
            Exiled.Events.Handlers.Player.Spawned += handlers.OnSpawning;
            Exiled.Events.Handlers.Player.PickingUpItem += handlers.OnPickingUpItem;
            Exiled.Events.Handlers.Player.ThrownProjectile += handlers.OnThrowingGrenade;
            Exiled.Events.Handlers.Player.DroppingItem += handlers.OnDroppingItem;
            Exiled.Events.Handlers.Player.UsedItem += handlers.OnUsingItem;
#else
            PluginAPI.Events.EventManager.RegisterEvents(this);
            PluginAPI.Events.EventManager.RegisterEvents<EventHandlers>(this);
#endif
            LoadTranslations();

            if(Extensions.HintCoroutineHandle == null || !Extensions.HintCoroutineHandle.Value.IsValid || !Extensions.HintCoroutineHandle.Value.IsRunning)
                Extensions.HintCoroutineHandle = Timing.RunCoroutine(Extensions.HintCoroutine());

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
#if EXILED
            Exiled.Events.Handlers.Player.Verified -= handlers.OnJoined;
            Exiled.Events.Handlers.Player.Died -= handlers.OnKill;
            Exiled.Events.Handlers.Server.RoundEnded -= handlers.OnRoundEnd;
            Exiled.Events.Handlers.Player.Escaping -= handlers.OnEscape;
            Exiled.Events.Handlers.Player.InteractingDoor -= handlers.OnInteractingDoor;
            Exiled.Events.Handlers.Scp914.UpgradingPickup -= handlers.OnScp914UpgradingItem;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem -= handlers.OnScp914UpgradingInventory;
            Exiled.Events.Handlers.Player.Spawned -= handlers.OnSpawning;
            Exiled.Events.Handlers.Player.PickingUpItem -= handlers.OnPickingUpItem;
            Exiled.Events.Handlers.Player.ThrownProjectile -= handlers.OnThrowingGrenade;
            Exiled.Events.Handlers.Player.DroppingItem -= handlers.OnDroppingItem;
            Exiled.Events.Handlers.Player.UsedItem -= handlers.OnUsingItem;
            handlers = null;
#endif

            _harmony.UnpatchAll(_harmony.Id);

            Instance = null;
            _harmony = null;
            db.Dispose();
            db = null;

#if EXILED
            base.OnDisabled();
#endif
        }

        public static string GetTranslation(string key)
        {
            DebugProgress("looking for key: " + key);
            DebugProgress($"Found key: {Instance.Translations.ContainsKey(key)}");
            return Instance.Translations.TryGetValue(key, out var translation) ? translation : null;
        }

        private void LoadTranslations()
        {
            try
            {
                var serializer = new Serializer();
                var deserializer = new Deserializer();
                if (!File.Exists(Config.SavePathTranslations))
                {
                    File.Create(Config.SavePathTranslations).Close();
                    using (TextWriter sr = new StreamWriter(Config.SavePathTranslations))
                    {
                        sr.Write(serializer.Serialize(Translations));
                    }

                    return;
                }
                
                using (TextReader sr = new StreamReader(Config.SavePathTranslations))
                {
                    Translations = deserializer.Deserialize<Dictionary<string, string>>(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                LogError("Could not load translations: " + e);
            }
        }

        public static void DebugProgress(string message)
        {
            if (Instance.Config.Debug)
                LogDebug(message);
        }

        public static void LogDebug(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Debug(message);
#else
            PluginAPI.Core.Log.Debug(message);
#endif
        }

        public static void LogWarn(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Warn(message);
#else
            PluginAPI.Core.Log.Warning(message);
#endif
        }

        public static void LogError(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Error(message);
#else
            PluginAPI.Core.Log.Error(message);
#endif
        }
    }
}