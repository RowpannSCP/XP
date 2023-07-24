using System;
using CommandSystem;

namespace XPSystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal sealed class Parent : ParentCommand
    {
        public Parent() => LoadGeneratedCommands();
        public override string Command { get; } =  "XPSystem";

        public override string[] Aliases { get; } =  new string[] { "xps" };

        public override string Description { get; } =  "Manipulates with players' XP and LVL values.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Leaderboard());
            RegisterCommand(new Set());
            RegisterCommand(new Get());
            RegisterCommand(new Refresh());
            RegisterCommand(new ShowHint());
            RegisterCommand(new PauseXPCommand());
            RegisterCommand(new Give());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Use: .xps (leaderboard | set | get | refresh | show | pause | give)";
            return false;
        }
    }
}
