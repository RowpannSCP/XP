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
            ReferenceHub ply = ReferenceHub.AllHubs.First(x => x.characterClassManager.UserId.Contains(sender.LogName));
            ply.ShowCustomHint("Test");
            response = "Shown hint!";
            return true;
        }
    }
}