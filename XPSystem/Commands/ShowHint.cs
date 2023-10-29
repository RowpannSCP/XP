namespace XPSystem.Commands
{
    using System;
    using System.Linq;
    using CommandSystem;
    using XPSystem.API;

    public class ShowHint : ICommand
    {
        public string Command { get; } =  "show";
        public string[] Aliases { get; } =  Array.Empty<string>();
        public string Description { get; } =  $"Show a example hint.";
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not CommandSender sender2)
            {
                response = "Sender is not a CommandSender";
                return false;
            }
            ReferenceHub ply = ReferenceHub.AllHubs.First(x => x.authManager.UserId.Contains(sender2.SenderId));
            ply.ShowCustomHint("Test");
            response = "Shown hint!";
            return true;
        }
    }
}