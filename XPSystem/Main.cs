using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using XPSystem.API.Serialization;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;

namespace XPSystem
{
    using Exiled.Events.Handlers;

    public class Main : Plugin<Config>
    {
        public override string Author { get; } = "Rowpann's Emperium, original by BrutoForceMaestro";
        public override string Name { get; } = "XPSystem";
        public override Version Version { get; } = new Version(1, 3, 1);
        public override Version RequiredExiledVersion { get; } = new Version(5, 2, 0);
        
        public static Main Instance { get; set; }
        public EventHandlers handlers;
        private Harmony _harmony;
        public LiteDatabase db;
        
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
            Scp914.UpgradingItem += handlers.OnScp914UpgradingItem;
            Scp914.UpgradingInventoryItem += handlers.OnScp914UpgradingInventory;

            _harmony.PatchAll();
            
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
            Scp914.UpgradingItem -= handlers.OnScp914UpgradingItem;
            Scp914.UpgradingInventoryItem -= handlers.OnScp914UpgradingInventory;
            
            _harmony.UnpatchAll(_harmony.Id);
            
            handlers = null;
            Instance = null;
            _harmony = null;
            db.Dispose();
            db = null;
            
            base.OnDisabled();
        }
    }
}