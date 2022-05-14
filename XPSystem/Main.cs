using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;

namespace XPSystem
{
    public class Main : Plugin<Config>
    {
        public override string Author { get; } = "Rowpann's Emperium";
        public override string Name { get; } = "XPSystem";
        public override Version Version { get; } = new Version(1, 2, 0);
        public override Version RequiredExiledVersion { get; } = new Version(5, 2, 0);
        
        public static Main Instance { get; set; }
        EventHandlers handlers;
        readonly Harmony harmony;

        public static Dictionary<string, PlayerLog> Players { get; set; } = new Dictionary<string, PlayerLog>();
        
        private void Deserialize()
        {
            if (!File.Exists(Instance.Config.SavePath))
            {
                JsonSerialization.Save();
                return;
            }
            JsonSerialization.Read();
        }
        public override void OnEnabled()
        {
            handlers = new EventHandlers();
            Instance = this;
            
            Player.Verified += handlers.OnJoined;
            Player.Dying += handlers.OnKill;
            Server.RoundEnded += handlers.OnRoundEnd;
            Player.Escaping += handlers.OnEscape;
            
            Deserialize();
            harmony.PatchAll();
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Verified -= handlers.OnJoined;
            Player.Dying -= handlers.OnKill;
            Server.RoundEnded -= handlers.OnRoundEnd;
            Player.Escaping -= handlers.OnEscape;
            
            handlers = null;
            harmony.UnpatchAll();
            
            base.OnDisabled();
        }
    }
}