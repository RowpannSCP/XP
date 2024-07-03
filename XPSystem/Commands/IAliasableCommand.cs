namespace XPSystem.Commands
{
    using CommandSystem;

    public interface IAliasableCommand : ICommand
    {
        public string CommandOverride { set; }
    }
}