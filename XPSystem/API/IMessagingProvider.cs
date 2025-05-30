namespace XPSystem.API
{
    using XPSystem.API.Player;

    /// <summary>
    /// Interface for a messaging provider, used to display messages to players.
    /// </summary>
    public interface IMessagingProvider
    {
        void DisplayMessage(BaseXPPlayer player, string message, float duration);
    }
}