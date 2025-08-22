namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using CommandSystem;
    using NorthwoodLib.Pools;
    using PlayerRoles;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.Config.Events;
    using XPSystem.Config.Events.Types;

    public class ShowMessageCommand : SanitizedInputCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xps.showmessage"))
            {
                response = "You do not have permission (xps.showmessage) to run this command.";
                return false;
            }

            if (arguments.Count < 1)
            {
                if (XPPlayer.TryGet(sender, out BaseXPPlayer? player) && player is XPPlayer xpPlayer)
                {
                    XPAPI.AddXPAndDisplayMessage(xpPlayer, 1, "debug");
                    response = "done.";
                    return true;
                }

                response = "Usage: showmessage (key) or showmessage (key) (subkeys)";
                return false;
            }

            string key = arguments.At(0);
            RoleTypeId role = RoleTypeId.None;

            if (key.StartsWith("default_"))
                key = key.Substring(8);
            else if (XPPlayer.TryGet(sender, out BaseXPPlayer? player))
                role = player.Role;

            XPECFile? file = XPECManager.GetFile(key, role);
            if (file == null)
            {
                response = "No such XPEC file.";
                return false;
            }

            List<object> subkeys = new List<object>();
            if (arguments.Count > 1)
            {
                var types = file.ParametersTypes;
                string[] subkeysStrings = arguments
                    .Skip(1)
                    .ToArray();

                for (int i = 0; i < subkeysStrings.Length; i++)
                {
                    string @string = subkeysStrings[i];
                    if (types.Length <= i)
                    {
                        subkeys.Add(@string);
                        continue;
                    }

                    bool success = false;
                    Exception? e = null;
                    var argTypes = types[i];
                    foreach (Type type in argTypes)
                    {
                        try
                        {
                            object converted = type.IsEnum
                                ? Enum.Parse(type, @string, true)
                                : Convert.ChangeType(@string, type);

                            if (converted != null)
                            {
                                subkeys.Add(converted);
                                success = true;
                                break;
                            }
                        }
                        catch (Exception e2)
                        {
                            e = e2;
                        }
                    }

                    if (!success)
                    {
                        response =
                            $"Could not convert argument at {i}: {@string} to any of the specified types, last error {e?.ToString() ?? "null"}";
                        return false;
                    }
                }
            }

            XPECItem? item = file.Get(subkeys.ToArray());
            if (item == null)
            {
                response = "Item null.";
                return false;
            }

            StringBuilder sb = StringBuilderPool.Shared.Rent();
            foreach (PropertyInfo property in item.GetType().GetProperties())
                sb.AppendLine($"{property.Name}: {property.GetValue(item)}");

            response = StringBuilderPool.Shared.ToStringReturn(sb);
            return true;
        }

        public override string Command { get; } = "showmessage";
        public override string[] Aliases { get; } = { "sm" };
        public override string Description { get; } = "Messaging and translation debug tool.";
    }
}