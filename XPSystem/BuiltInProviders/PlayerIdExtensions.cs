namespace XPSystem.BuiltInProviders
{
    using XPSystem.API.StorageProviders.Models;

    public static class PlayerIdExtensions
    {
        public static object GetId(this IPlayerId playerId) => playerId.GetId<object>();
        public static T GetId<T>(this IPlayerId playerId) => ((IPlayerId<T>)playerId).Id;
    }
}