using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Cache.Factory;
using Cache.ConfigModels;

namespace Cache.Extensions
{
    public static class RedisServiceCollectionExtensions
    {
        /// <summary>
        /// Initializes a Redis connection and registers it for dependency injection.
        /// Adds Redis cache to the application's service collection.
        /// </summary>
        /// <param name="services">The service collection to register with.</param>
        /// <param name="config">The application configuration containing the Redis connection string.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the Redis connection string is missing or empty.</exception>

        public static IServiceCollection AddRedisCacheLibrary(this IServiceCollection services, IConfiguration config)
        {
            string secretKey = "REDIS-CONNECTION-STRING";
            string? keyVaultUrl = config["KeyVault:Url"];

            KeyVaultConfigs keyVaultConfigs = new KeyVaultConfigs { Url = keyVaultUrl };
            KeyVaultConnectionService.Initialize(keyVaultConfigs);
            KeyVaultResponse response = KeyVaultConnectionService.GetSecret(secretKey);

            if (response.IsSuccess)
            {
                string redisConnectionString = response.Secret;
                RedisConnectionFactory.Initialize(response.Secret);
                services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
                services.Configure<CacheSettings>(config.GetSection("CacheSettings"));
                return services;
            }
            else
            {
                throw new ArgumentNullException("Failed to retrieve the Redis connection string from Key Vault.");
            }
        }
    }
}
