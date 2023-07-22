using System;
using System.Linq;
using CommandSystem;
using XPSystem.API.Serialization;

namespace XPSystem.Commands
{
    using XPSystem.API;

    public class Leaderboard : ICommand
    {
        public string Command { get; } =  "leaderboard";
        public string[] Aliases { get; } =  new string[] { "lb" };
        public string Description { get; } =  "Players, sorted by their LV (Level of Violence). Use: XPSystem leaderboard (amount)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionInternal("xps.get"))
            {
                response = "You don't have permission (xps.get) to use this command.";
                return false;
            }
            if (arguments.Count == 0)
            {
                response = GetTopPlayers(10);
                return true;
            }

            if (int.TryParse(arguments.At(0), out var amount) && amount > 0)
            {
                response = GetTopPlayers(amount);
                return true;
            }

            response = "Invalid players amount.";
            return false;
        }

        private string GetTopPlayers(int amount)
        {
            var sorted = Main.Instance.db.GetCollection<PlayerLog>("Players").FindAll().OrderByDescending(o => o.LVL);
            var players = sorted.Take(amount);
            string str = "";
            int index = 1;
            foreach (var log in players)
            {
                str += $"{index}. ({log.ID}) : LVL{log.LVL}, XP: {log.XP}\n";
                index++;
            }
            return str;
        }
    }
}
