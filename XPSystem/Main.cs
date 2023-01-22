namespace XPSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Exiled.API.Features;
    using HarmonyLib;
    using LiteDB;
    using MEC;
    using XPSystem.API;
    using YamlDotNet.Serialization;
    using Player = Exiled.Events.Handlers.Player;
    using Scp914 = Exiled.Events.Handlers.Scp914;
    using Server = Exiled.Events.Handlers.Server;

    public class Main : Plugin<Config>
    {
        public override string Author { get; } = "Rowpann's Emperium, original by BrutoForceMaestro";
        public override string Name { get; } = "XPSystem";
        public override Version Version { get; } = new Version(1, 7, 3);
        public override Version RequiredExiledVersion { get; } = new Version(6, 0, 0);
        
        public static Main Instance { get; set; }
        public EventHandlers handlers;
        private Harmony _harmony;
        public LiteDatabase db;
        
        public Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            ["ExampleKey"] = "ExampleValue",
            ["ExampleKey2"] = "ExampleValue",
        };

        public override void OnEnabled()
        {
            db = new LiteDatabase(Config.SavePath);
            handlers = new EventHandlers();
            Instance = this;
            _harmony = new Harmony($"XPSystem - {DateTime.Now.Ticks}");
            
            Player.Verified += handlers.OnJoined;
            Player.Dying += handlers.OnKill;
            Server.RoundEnded += handlers.OnRoundEnd;
            Player.Escaping += handlers.OnEscape;
            Player.Destroying += handlers.OnLeaving;
            Player.InteractingDoor += handlers.OnInteractingDoor;
            Scp914.UpgradingPickup += handlers.OnScp914UpgradingItem;
            Scp914.UpgradingInventoryItem += handlers.OnScp914UpgradingInventory;
            Player.Spawned += handlers.OnSpawning;
            Player.PickingUpItem += handlers.OnPickingUpItem;
            Player.ThrownProjectile += handlers.OnThrowingGrenade;
            Player.DroppingItem += handlers.OnDroppingItem;
            
            LoadTranslations();

            _harmony.PatchAll();
            if(Extensions.HintCoroutineHandle == null || !Extensions.HintCoroutineHandle.Value.IsValid || !Extensions.HintCoroutineHandle.Value.IsRunning)
                Extensions.HintCoroutineHandle = Timing.RunCoroutine(Extensions.HintCoroutine());
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Verified -= handlers.OnJoined;
            Player.Dying -= handlers.OnKill;
            Server.RoundEnded -= handlers.OnRoundEnd;
            Player.Escaping -= handlers.OnEscape;
            Player.Destroying -= handlers.OnLeaving;
            Player.InteractingDoor -= handlers.OnInteractingDoor;
            Scp914.UpgradingPickup -= handlers.OnScp914UpgradingItem;
            Scp914.UpgradingInventoryItem -= handlers.OnScp914UpgradingInventory;
            Player.Spawned -= handlers.OnSpawning;
            Player.PickingUpItem -= handlers.OnPickingUpItem;
            Player.ThrownProjectile -= handlers.OnThrowingGrenade;
            Player.DroppingItem -= handlers.OnDroppingItem;
            
            _harmony.UnpatchAll(_harmony.Id);
            
            handlers = null;
            Instance = null;
            _harmony = null;
            db.Dispose();
            db = null;
            
            base.OnDisabled();
        }

        public static string GetTranslation(string key)
        {
            if (Instance.Config.Debug)
            {
                Log.Debug("looking for key: " + key);
                Log.Debug($"Found key: {Instance.Translations.ContainsKey(key)}");
            }
            return Instance.Translations.ContainsKey(key) ? Instance.Translations[key] : null;
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
                Log.Error("Could not load translations: " + e);
            }
        }
    }
}