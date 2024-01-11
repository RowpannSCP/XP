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
        private const string VersionString = "1.11.1";

        public static bool EnabledNick = false;
        public static bool EnabledRank = false;
        public static bool Paused = false;
        public static Main Instance { get; set; }
        public EventHandlers Handlers;
        private Harmony _harmony;
        public LiteDatabase db;

        public Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            ["ExampleKey"] = "ExampleValue",
            ["ExampleKey2"] = "ExampleValue",
        };
        public Dictionary<string, int> UsedKeys = new Dictionary<string, int>();

#if EXILED
        private static readonly int[] split = VersionString.Split('.').Select(x => Convert.ToInt32(x)).ToArray();
        public override string Author { get; } = "Rowpann's Emperium, original by BrutoForceMaestro";
        public override string Name { get; } = "XPSystem";
        public override Version Version { get; } = new Version(split[0], split[1], split[2]);
        public override Version RequiredExiledVersion { get; } = new Version(8, 3, 9);
#else
        [PluginAPI.Core.Attributes.PluginConfig]
        public Config Config;
#endif

#if EXILED
        public override void OnEnabled()
#else
        [PluginAPI.Core.Attributes.PluginEntryPoint("xpsystem", VersionString, "xp plugin", "Rowpann's Emperium, original by BrutoForceMaestro")]
        public void OnEnabled()
#endif
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Config.SavePath)!);
            db = new LiteDatabase(Config.SavePath);
            Instance = this;
            _harmony = new Harmony($"XPSystem - {DateTime.Now.Ticks}");
            _harmony.PatchAll();

#if EXILED
            Handlers = new EventHandlers();
            Exiled.Events.Handlers.Player.Verified += Handlers.OnJoined;
            Exiled.Events.Handlers.Player.Died += Handlers.OnKill;
            Exiled.Events.Handlers.Server.RoundEnded += Handlers.OnRoundEnd;
            Exiled.Events.Handlers.Player.Escaping += Handlers.OnEscape;
            Exiled.Events.Handlers.Player.InteractingDoor += Handlers.OnInteractingDoor;
            Exiled.Events.Handlers.Scp914.UpgradingPickup += Handlers.OnScp914UpgradingItem;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem += Handlers.OnScp914UpgradingInventory;
            Exiled.Events.Handlers.Player.Spawned += Handlers.OnSpawning;
            Exiled.Events.Handlers.Player.PickingUpItem += Handlers.OnPickingUpItem;
            Exiled.Events.Handlers.Player.ThrownProjectile += Handlers.OnThrowingGrenade;
            Exiled.Events.Handlers.Player.DroppingItem += Handlers.OnDroppingItem;
            Exiled.Events.Handlers.Player.UsedItem += Handlers.OnUsingItem;

            if(Extensions.HintCoroutineHandle == null || !Extensions.HintCoroutineHandle.Value.IsValid || !Extensions.HintCoroutineHandle.Value.IsRunning)
                Extensions.HintCoroutineHandle = Timing.RunCoroutine(Extensions.HintCoroutine());
#else
            PluginAPI.Events.EventManager.RegisterEvents(this);
            PluginAPI.Events.EventManager.RegisterEvents(this, Handlers = new EventHandlers());
#endif
            LoadTranslations();

            if (Config.EnableNickMods)
                EnabledNick = true;
            if (Config.EnableBadges)
                EnabledRank = true;

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
            Exiled.Events.Handlers.Player.Verified -= Handlers.OnJoined;
            Exiled.Events.Handlers.Player.Died -= Handlers.OnKill;
            Exiled.Events.Handlers.Server.RoundEnded -= Handlers.OnRoundEnd;
            Exiled.Events.Handlers.Player.Escaping -= Handlers.OnEscape;
            Exiled.Events.Handlers.Player.InteractingDoor -= Handlers.OnInteractingDoor;
            Exiled.Events.Handlers.Scp914.UpgradingPickup -= Handlers.OnScp914UpgradingItem;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem -= Handlers.OnScp914UpgradingInventory;
            Exiled.Events.Handlers.Player.Spawned -= Handlers.OnSpawning;
            Exiled.Events.Handlers.Player.PickingUpItem -= Handlers.OnPickingUpItem;
            Exiled.Events.Handlers.Player.ThrownProjectile -= Handlers.OnThrowingGrenade;
            Exiled.Events.Handlers.Player.DroppingItem -= Handlers.OnDroppingItem;
            Exiled.Events.Handlers.Player.UsedItem -= Handlers.OnUsingItem;
            Handlers = null;
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
            if (Instance?.Config.LogXPGainedMethods ?? false)
            {
                if (Instance.UsedKeys.ContainsKey(key))
                    Instance.UsedKeys[key]++;
                else
                    Instance.UsedKeys.Add(key, 1);
            }

            DebugProgress("looking for key: " + key);
            DebugProgress($"Found key: {Instance.Translations.ContainsKey(key)}");
            return Instance.Translations.TryGetValue(key, out var translation) ? translation : null;
        }

#if EXILED
        public bool CanGetXP(Exiled.API.Features.Player ply, string key, ushort itemSerial)
        {
            var item = Exiled.API.Features.Items.Item.Get(itemSerial);
            if (item == null) return false;
            if (!Config.UseTimer) return true;
            if (Config.TimerUseItemType)
            {
                if (ply.SessionVariables.TryGetValue($"xpitemusedtype{key}", out var obj) && obj is Dictionary<ItemType, DateTime> list)
                {
                    if (list.TryGetValue(item.Type, out var time))
                    {
                        if (DateTime.Now - time > TimeSpan.FromSeconds(Config.TimerDuration))
                        {
                            list[item.Type] = DateTime.Now;
                            return true;
                        }
                        return false;
                    }
                    list.Add(item.Type, DateTime.Now);
                    return true;
                }
                ply.SessionVariables.Add($"xpitemusedtype{key}", new Dictionary<ItemType, DateTime> { [item.Type] = DateTime.Now });
                return true;
            }
            else
            {
                if (ply.SessionVariables.TryGetValue($"xpitemuseditem{key}", out var obj) && obj is Dictionary<ushort, DateTime> list)
                {
                    if (list.TryGetValue(item.Serial, out var time))
                    {
                        if (DateTime.Now - time > TimeSpan.FromSeconds(Config.TimerDuration))
                        {
                            list[item.Serial] = DateTime.Now;
                            return true;
                        }
                        return false;
                    }
                    list.Add(item.Serial, DateTime.Now);
                    return true;
                }
                ply.SessionVariables.Add($"xpitemuseditem{key}", new Dictionary<ushort, DateTime> { [item.Serial] = DateTime.Now });
                return true;
            }
        }
#else
        public bool CanGetXP(PluginAPI.Core.Player ply, string key, ushort serial, ItemType itemType)
        {
            if (serial == 0 || itemType == ItemType.None) return false;
            if (!Config.UseTimer) return true;
            if (Config.TimerUseItemType)
            {
                if (ply.TemporaryData.StoredData.TryGetValue($"xpitemusedtype{key}", out var obj) && obj is Dictionary<ItemType, DateTime> list)
                {
                    if (list.TryGetValue(itemType, out var time))
                    {
                        if (DateTime.Now - time > TimeSpan.FromSeconds(Config.TimerDuration))
                        {
                            list[itemType] = DateTime.Now;
                            return true;
                        }
                        return false;
                    }
                    list.Add(itemType, DateTime.Now);
                    return true;
                }
                ply.TemporaryData.StoredData.Add($"xpitemusedtype{key}", new Dictionary<ItemType, DateTime> { [itemType] = DateTime.Now });
                return true;
            }
            else
            {
                if (ply.TemporaryData.StoredData.TryGetValue($"xpitemuseditem{key}", out var obj) && obj is Dictionary<ushort, DateTime> list)
                {
                    if (list.TryGetValue(serial, out var time))
                    {
                        if (DateTime.Now - time > TimeSpan.FromSeconds(Config.TimerDuration))
                        {
                            list[serial] = DateTime.Now;
                            return true;
                        }
                        return false;
                    }
                    list.Add(serial, DateTime.Now);
                    return true;
                }
                ply.TemporaryData.StoredData.Add($"xpitemuseditem{key}", new Dictionary<ushort, DateTime> { [serial] = DateTime.Now });
                return true;
            }
        }
#endif

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