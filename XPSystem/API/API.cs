using XPSystem.API.Serialization;

namespace XPSystem.API
{
    public static class API
    {
        public static bool TryGetId(string id, out PlayerLog log)
        {
            log = Main.Instance.db.GetCollection<PlayerLog>("Players").FindById(id);
            return log != null;
        }
    }
}