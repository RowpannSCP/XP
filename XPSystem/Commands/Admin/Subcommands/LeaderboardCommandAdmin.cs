namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class LeaderboardCommandAdmin : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xps.leaderboard"))
            {
                response = "You don't have permission (xps.leaderboard) to use this command.";
                return false;
            }

            XPAPI.EnsureStorageProviderValid();

            int amount = 5;
            if (arguments.Count > 0)
            {
                if (!int.TryParse(arguments.At(0), out amount))
                {
                    response = "Invalid amount.";
                    return false;
                }

                if (amount < 1)
                {
                    response = "Amount must be at least 1.";
                    return false;
                }
            }
            
            response = XPAPI.StorageProvider.GetTopPlayers(amount)
                .FormatLeaderboard();
            return true;
        }

        public string Command { get; } = "leaderboard";
        public string[] Aliases { get; } = new[] { "lb" };
        public string Description { get; } = "Get the top players.";
    }
}