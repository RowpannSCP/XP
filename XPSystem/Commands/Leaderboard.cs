using System;
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
                response = GetTopPlayersString(10);
                return true;
            }

            if (int.TryParse(arguments.At(0), out var amount) && amount > 0)
            {
                response = GetTopPlayersString(amount);
                return true;
            }

            response = "Invalid players amount.";
            return false;
        }

        private static string GetTopPlayersString(int amount)
        {
            string str = "";
            int index = 1;
            foreach (var log in GetTopPlayers(amount))
            {
                str += $"{index}. ({log.ID}) : LVL{log.LVL}, XP: {log.XP}\n";
                index++;
            }

            return str;
        }

        /// <summary>
        /// public cause notintense wants it
        /// </summary>
        public static PlayerLog[] GetTopPlayers(int amount)
        {
            var sorted = API.PlayerLogCollection
                .Query()
                .OrderByDescending(x => x.LVL)
                .Limit(amount);
            return sorted.ToArray();
        }
    }
}
