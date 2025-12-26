namespace XPSystem.Commands.Client.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class LeaderboardCommandClient : SanitizedInputCommand, IAliasableCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
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

                if (amount > XPAPI.Config.LeaderboardMaxLength)
                {
                    response = $"Amount may not exceed {XPAPI.Config.LeaderboardMaxLength}.";
                    return false;
                }
            }

            response = XPAPI.StorageProvider!.GetTopPlayers(amount)
                .FormatLeaderboard();
            return true;
        }

        public string CommandOverride { get; set; } = "leaderboard";
        public override string Command => CommandOverride;
        public override string[] Aliases { get; } = Array.Empty<string>();
        public override string Description { get; } = "Get the top players.";
    }
}