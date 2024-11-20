namespace XPSystem.BuiltInProviders
{
    using XPSystem.API.StorageProviders.Models;

    public static class PlayerIdExtensions
    {
        public static object GetId(this IPlayerId<object> playerId) => playerId.GetId<object>();
        public static T GetId<T>(this IPlayerId<object> playerId) => ((IPlayerId<T>)playerId).Id;
    }
}