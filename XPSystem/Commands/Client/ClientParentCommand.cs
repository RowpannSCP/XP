namespace XPSystem.Commands.User
{
    using System;
    using System.Linq;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.Commands.Client.Subcommands;

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class ClientParentCommand : ParentCommand
    {
        public ClientParentCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            if (XPAPI.Config.EnableGetXPCommand)
                RegisterCommand(new GetCommandClient());
            if (XPAPI.Config.EnableLeaderboardCommand)
                RegisterCommand(new LeaderboardCommandClient());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = $"Usage: xp ({string.Join(" | ", Commands.Select(x => x.Key))})";
            return false;
        }

        public override string Command { get; } = "xp";
        public override string[] Aliases { get; } = new[] { "xps" };
        public override string Description { get; } = "Client parent command for XPSystem.";
    }
}