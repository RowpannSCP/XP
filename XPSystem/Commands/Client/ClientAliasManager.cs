namespace XPSystem.Commands.Client
{
    using System.Collections.Generic;
    using RemoteAdmin;
    using static XPSystem.API.XPAPI;

    public static class ClientAliasManager
    {
        public static List<IAliasableCommand> Aliases { get; } = new List<IAliasableCommand>();

        public static void RegisterAliases()
        {
            if (!string.IsNullOrWhiteSpace(Config.ClientGetCommandAlias))
                QueryProcessor.DotCommandHandler.RegisterCommand(CreateAlias(Config.ClientGetCommandAlias, new Subcommands.GetCommandClient()));

            if (!string.IsNullOrWhiteSpace(Config.ClientLeaderboardCommandAlias))
                QueryProcessor.DotCommandHandler.RegisterCommand(CreateAlias(Config.ClientLeaderboardCommandAlias, new Subcommands.LeaderboardCommandClient()));
        }

        public static void UnregisterAliases()
        {
            foreach (IAliasableCommand alias in Aliases)
                QueryProcessor.DotCommandHandler.UnregisterCommand(alias);
        }

        private static IAliasableCommand CreateAlias(string alias, IAliasableCommand command)
        {
            command.CommandOverride = alias;
            Aliases.Add(command);
            return command;
        }
    }
}