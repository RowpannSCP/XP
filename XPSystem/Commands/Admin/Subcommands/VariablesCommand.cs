namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using System.Text;
    using CommandSystem;
    using NorthwoodLib.Pools;
    using XPSystem.API;

    public class VariablesCommand : SanitizedInputCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.variables", out XPPlayer player))
            {
                response = "You don't have permission (xps.variables) to use this command.";
                return false;
            }

            if (arguments.Count > 0)
            {
                if (!XPPlayer.TryGet(arguments.At(0), out player))
                {
                    response = "Player not found.";
                    return false;
                }
            }

            if (player.Variables.Count == 0)
            {
                response = "No variables.";
                return true;
            }

            StringBuilder sb = StringBuilderPool.Shared.Rent();
            sb.AppendLine("Variables:");

            foreach (var kvp in player.Variables)
                sb.AppendLine($"{kvp.Key}: {kvp.Value.Value?.ToString() ?? "null"}");

            response = StringBuilderPool.Shared.ToStringReturn(sb);
            return true;
        }

        public override string Command { get; } = "variables";
        public override string[] Aliases { get; } = new[] { "v" };
        public override string Description { get; } = "Shows your variables.";
    }
}