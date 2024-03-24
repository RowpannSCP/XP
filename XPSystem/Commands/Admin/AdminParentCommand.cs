namespace XPSystem.Commands.Admin
{
    using System;
    using CommandSystem;
    using XPSystem.Commands.Admin.Subcommands;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class AdminParentCommand : ParentCommand
    {
        public AdminParentCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new GetCommandAdmin());
            RegisterCommand(new GiveCommand());
            RegisterCommand(new SetCommand());
            RegisterCommand(new LeaderboardCommandAdmin());
            RegisterCommand(new PauseCommand());
            RegisterCommand(new RefreshCommand());
            RegisterCommand(new ShowMessageCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = $"Usage: xp ({string.Join(" | ", Commands)})";
            return false;
        }

        public override string Command { get; } = "xp";
        public override string[] Aliases { get; } = new[] { "xps" };
        public override string Description { get; } = "Admin parent command for XPSystem.";
    }
}