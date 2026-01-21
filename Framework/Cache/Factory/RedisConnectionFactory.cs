using StackExchange.Redis;

namespace Cache.Factory
{
    internal class RedisConnectionFactory
    {
        private static Lazy<ConnectionMultiplexer>? _lazyConnection;

        public static ConnectionMultiplexer Connection =>
               _lazyConnection?.Value
                   ?? throw new InvalidOperationException("Redis not initialized.");

        /// <summary>
        /// Initializes the Redis connection.
        /// </summary>
        public static void Initialize(string connectionString)
        {
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                ConnectionMultiplexer.Connect(connectionString));
        }

        /// <summary>
        /// Gets the Redis database.
        /// </summary>
        /// <returns>An <see cref="IDatabase"/> instance used to perform Redis operations.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the factory has not been initialized via <see cref="Initialize(string)"/>.
        /// </exception>
        public static IDatabase GetDatabase()
        {
            if (_lazyConnection == null)
                throw new InvalidOperationException("RedisConnectionFactory is not initialized.");

            return _lazyConnection.Value.GetDatabase();
        }
    }
}
