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
        EventHandlers handlers;
        readonly Harmony harmony = new Harmony("com.nutmaster.rankchangepatch");
        public static Main Instance { get; set; }
        public static Dictionary<string, PlayerLog> Players { get; set; } = new Dictionary<string, PlayerLog>();
        public override Version Version => new Version(1, 2, 0);
        public override Version RequiredExiledVersion => new Version(5, 0, 0);

        private void Deserialize()
        {
            if (!File.Exists(Instance.Config.SavePath)) // prevent filenotfound
            {
                JsonSerialization.Save();
                return;
            }
            JsonSerialization.Read();
        }
        public override void OnEnabled()
        {
            handlers = new EventHandlers();
            Player.Verified += handlers.OnJoined;
            Player.Dying += handlers.OnKill;
            Server.RoundEnded += handlers.OnRoundEnd;
            Player.Escaping += handlers.OnEscape;
            Instance = this;
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