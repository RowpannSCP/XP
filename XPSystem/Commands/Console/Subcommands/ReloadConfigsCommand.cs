namespace XPSystem.Commands.Console.Subcommands
{
    using System;
    using CommandSystem;

    public class ReloadConfigsCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Main.Instance.LoadExtraConfigs();
            response = "Extra (non main, use your pluginloader to reload) configs reloaded.";
            return true;
        }

        public string Command { get; } = "reloadconfigs";
        public string[] Aliases { get; } = new[] { "reload" };
        public string Description { get; } = "Reloads the extra config files.";
    }
}