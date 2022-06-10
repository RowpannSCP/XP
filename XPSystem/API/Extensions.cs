using System.Collections.Generic;
using XPSystem.API.Serialization;

namespace XPSystem.API
{
    public static class Extensions
    {
        public static IEnumerable<PlayerLogSerializable> GetLogSerializables(this IEnumerable<PlayerLog> log)
        {
            return log;
        }
        
        public static IDictionary<string, PlayerLogSerializable> GetLogSerializables(this IDictionary<string, PlayerLog> log)
        {
            var result = new Dictionary<string, PlayerLogSerializable>();
            foreach (var kvp in log)
            {
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }
    }
}