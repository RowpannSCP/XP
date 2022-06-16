using XPSystem.API.Serialization;

namespace XPSystem.API
{
    public static class API
    {
        public static bool TryGetLog(string id, out PlayerLog log)
        {
            log = Main.Instance.db.GetCollection<PlayerLog>("Players").FindById(id);
            return log != null;
        }
    }
}