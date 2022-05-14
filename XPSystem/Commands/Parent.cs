using CommandSystem;
using System;

namespace XPSystem
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class Parent : ParentCommand
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
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Use: .xps (leaderboard | set | get)";
            return false;
        }
    }
}
