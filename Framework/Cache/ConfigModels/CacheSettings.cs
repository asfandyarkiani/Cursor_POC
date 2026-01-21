namespace Cache.ConfigModels
{
    public class CacheSettings
    {
        public int DefaultTTL { get; set; } = 15;
        public Dictionary<string, int> TTLsInMinutes { get; set; } = new();

        public int GetTTLForMethod(string methodName)
        {
            if (TTLsInMinutes != null && TTLsInMinutes.TryGetValue(methodName, out int ttl) && ttl > 0)
            {
                return ttl;
            }
            return DefaultTTL;
        }
    }
}
