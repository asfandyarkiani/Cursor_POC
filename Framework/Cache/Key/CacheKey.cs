using Cache.Enums;
using Cache.Factory;
using StackExchange.Redis;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Cache.Key
{
    public class CacheKey
    {
        private RedisKey _key;

        public RedisKey InternalKey { get { return _key; } }
        public CacheKeySettings Settings { get; set; }

        private CacheKey(string key, CacheKeySettings _cacheKeySettings = null)
        {
            _key = new RedisKey(key);
            Settings = _cacheKeySettings;
        }

        /// <summary>
        /// Creates a cache key from property values and an optional namespace.
        /// </summary>
        public static CacheKey CreateCacheKey(IDictionary<PropertyInfo, object> _propertiesForCacheKey, string _namespace = "", CacheKeySettings _cacheKeySettings = null)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(_namespace))
            {
                sb.Append(_namespace + ":");
            }

            List<PropertyInfo> dictionaryKeys = new List<PropertyInfo>(_propertiesForCacheKey.Keys);

            for (int i = 0; i < dictionaryKeys.Count; i++)
            {
                PropertyInfo key = dictionaryKeys[i];
                object value = _propertiesForCacheKey[key];
                if (i == dictionaryKeys.Count - 1)
                {
                    sb.Append(key.Name + ":" + value);
                }
                else
                {
                    sb.Append(key.Name + ":" + value + ":");
                }
            }
            return new CacheKey(sb.ToString(), _cacheKeySettings);
        }

        /// <summary>
        /// Creates a cache key from a list of string segments and an optional namespace.
        /// </summary>
        /// <remarks>
        /// The cache key is constructed by joining the non-empty segments with colons.
        /// An optional namespace will be prepended to the key, also separated by a colon.
        /// 
        /// <b>Example format:</b> 
        /// <namespace>:<object_type>:<identifier>
        /// i.e - ecommerce:prod:order:12345:item:67890
        /// </remarks>

        public static CacheKey CreateCacheKey(List<string> keys, string _namespace = "", CacheKeySettings _cacheKeySettings = null)
        {
            if (keys == null || !keys.Any())
                throw new ArgumentException("Keys list cannot be null or empty.", nameof(keys));

            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_namespace))
            {
                sb.Append(_namespace).Append(":");
            }

            List<string> cleanKeys = keys.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();
            sb.Append(string.Join(":", cleanKeys));

            return new CacheKey(sb.ToString(), _cacheKeySettings);
        }

        /// <summary>
        /// Stores a value in Redis for this key.
        /// </summary>
        public async Task<bool> SetAsync<T>(T value)
        {
            if (value == null)
                return false;

            IDatabase db = RedisConnectionFactory.GetDatabase();
            string json = JsonSerializer.Serialize(value);
            if (Settings != null)
            {
                return await db.StringSetAsync(_key, json, Settings.TimeToLive);
            }
            else
            {
                return await db.StringSetAsync(_key, json);
            }
        }

        // <summary>
        /// Stores a string value in Redis for this key.
        /// </summary>
        public async Task<bool> SetStringAsync(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            IDatabase db = RedisConnectionFactory.GetDatabase();

            if (Settings?.TimeToLive != null)
            {
                return await db.StringSetAsync(_key, value, Settings.TimeToLive);
            }
            else
            {
                return await db.StringSetAsync(_key, value);
            }
        }

        /// <summary>
        /// Retrieves a value from Redis for this key.
        /// </summary>
        public async Task<T?> GetAsync<T>()
        {
            IDatabase db = RedisConnectionFactory.GetDatabase();
            RedisValue json = await db.StringGetAsync(_key);

            if (json.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(json!);
        }

        /// <summary>
        /// Retrieves a string value from Redis for this key.
        /// </summary>
        public async Task<string?> GetStringAsync()
        {
            IDatabase db = RedisConnectionFactory.GetDatabase();
            RedisValue value = await db.StringGetAsync(_key);

            if (value.IsNullOrEmpty)
                return default;

            return value.ToString();
        }

        /// <summary>
        /// Deletes this key from Redis.
        /// </summary>
        public async Task DeleteAsync()
        {
            IDatabase db = RedisConnectionFactory.GetDatabase();
            await db.KeyDeleteAsync(_key);
        }

        /// <summary>
        /// Checks if this key exists in Redis.
        /// </summary>
        /// <returns>True if the key exists, otherwise false.</returns>
        public async Task<bool> ExistsAsync()
        {
            IDatabase db = RedisConnectionFactory.GetDatabase();

            return await db.KeyExistsAsync(_key);
        }

        public static List<string> GetAllKeys()
        {
            List<string> result = new List<string>();
            EndPoint[] endpoints = RedisConnectionFactory.Connection.GetEndPoints();

            foreach (EndPoint endpoint in endpoints)
            {
                IServer server = RedisConnectionFactory.Connection.GetServer(endpoint);
                if (!server.IsConnected)
                    continue;

                // Get all keys
                foreach (RedisKey key in server.Keys())
                {
                    result.Add(key.ToString());
                }
            }

            return result;
        }

        public static List<string> FindKeys(string value, KeyMatchType matchType = KeyMatchType.Contains)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
                return result;

            // Determine Redis pattern based on match type
            string pattern = matchType switch
            {
                KeyMatchType.StartsWith => $"{value}*",
                KeyMatchType.EndsWith => $"*{value}",
                KeyMatchType.Contains => $"*{value}*",
                _ => $"*{value}*"
            };

            EndPoint[] endpoints = RedisConnectionFactory.Connection.GetEndPoints();
            foreach (EndPoint endpoint in endpoints)
            {
                IServer server = RedisConnectionFactory.Connection.GetServer(endpoint);
                if (!server.IsConnected)
                    continue;

                foreach (RedisKey key in server.Keys(pattern: pattern))
                {
                    result.Add(key.ToString());
                }
            }

            return result;
        }
    }
}
