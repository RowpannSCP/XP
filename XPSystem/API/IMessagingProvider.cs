namespace XPSystem.API
{
    /// <summary>
    /// Interface for a messaging provider, used to display messages to players.
    /// </summary>
    public interface IMessagingProvider
    {
        void DisplayMessage(XPPlayer player, string message);
    }
}