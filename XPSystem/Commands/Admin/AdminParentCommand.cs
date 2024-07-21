namespace XPSystem.Commands.Admin
{
    using System;
    using System.Linq;
    using CommandSystem;
    using XPSystem.Commands.Admin.Subcommands;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class AdminParentCommand : ParentCommand
    {
        public AdminParentCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new ClearCacheCommand());
            RegisterCommand(new GetCommandAdmin());
            RegisterCommand(new GiveCommand());
            RegisterCommand(new LeaderboardCommandAdmin());
            RegisterCommand(new MultiplierCommand());
            RegisterCommand(new PauseCommand());
            RegisterCommand(new RefreshCommand());
            RegisterCommand(new SetCommand());
            RegisterCommand(new SetLevelCommand());
            RegisterCommand(new ShowMessageCommand());
            RegisterCommand(new VariablesCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = $"Usage: xp ({string.Join(" | ", Commands.Select(x => x.Key))})";
            return false;
        }

        public override string Command { get; } = "xp";
        public override string[] Aliases { get; } = new[] { "xps" };
        public override string Description { get; } = "Admin parent command for XPSystem.";
    }
}