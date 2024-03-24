namespace XPSystem.Commands.Admin
{
    using System;
    using CommandSystem;
    using XPSystem.Commands.Console.Subcommands;

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class ConsoleParentCommand : ParentCommand
    {
        public ConsoleParentCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new DeleteEverythingCommand());
            RegisterCommand(new MigrateCommand());
            
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = $"Usage: xp ({string.Join(" | ", Commands)})";
            return false;
        }

        public override string Command { get; } = "xp";
        public override string[] Aliases { get; } = new[] { "xps" };
        public override string Description { get; } = "Console parent command for XPSystem.";
    }
}