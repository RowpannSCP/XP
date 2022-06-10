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
    public class Main : Plugin<Config>
    {
        public override string Author { get; } = "Rowpann's Emperium, original by BrutoForceMaestro";
        public override string Name { get; } = "XPSystem";
        public override Version Version { get; } = new Version(1, 2, 0);
        public override Version RequiredExiledVersion { get; } = new Version(5, 2, 0);
        
        public static Main Instance { get; set; }
        EventHandlers handlers;
        private Harmony _harmony;

        public static Dictionary<string, PlayerLog> Players { get; set; } = new Dictionary<string, PlayerLog>();
        
        private void Deserialize()
        {
            if (!File.Exists(Instance.Config.SavePath))
            {
                DBUtils.Save();
                return;
            }
            DBUtils.Read();
        }
        public override void OnEnabled()
        {
            handlers = new EventHandlers();
            Instance = this;
            _harmony = new Harmony($"XPSystem - {DateTime.Now.Ticks}");
            
            Player.Verified += handlers.OnJoined;
            Player.Dying += handlers.OnKill;
            Server.RoundEnded += handlers.OnRoundEnd;
            Player.Escaping += handlers.OnEscape;
            
            Deserialize();
            _harmony.PatchAll();
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Verified -= handlers.OnJoined;
            Player.Dying -= handlers.OnKill;
            Server.RoundEnded -= handlers.OnRoundEnd;
            Player.Escaping -= handlers.OnEscape;
            
            _harmony.UnpatchAll(_harmony.Id);
            
            handlers = null;
            Instance = null;
            _harmony = null;

            base.OnDisabled();
        }
    }
}