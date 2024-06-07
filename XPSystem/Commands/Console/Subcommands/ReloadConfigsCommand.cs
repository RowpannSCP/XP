namespace XPSystem.Commands.Console.Subcommands
{
    using System;
    using CommandSystem;

    public class ReloadConfigsCommand : SanitizedInputCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Main.Instance.LoadExtraConfigs();
            response = "Extra (non main, use your pluginloader to reload) configs reloaded.";
            return true;
        }

        public override string Command { get; } = "reloadconfigs";
        public override string[] Aliases { get; } = new[] { "reload" };
        public override string Description { get; } = "Reloads the extra config files.";
    }
}