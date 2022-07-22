namespace XPSystem.Commands
{
    using System;
    using API;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;

    public class ShowHint : ICommand
    {
        public string Command { get; } =  "show";
        public string[] Aliases { get; } =  Array.Empty<string>();
        public string Description { get; } =  $"Show a example hint.";
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player ply = Player.Get(sender);
            ply.ShowCustomHint("Test");
            response = "Shown hint!";
            return true;
        }
    }
}