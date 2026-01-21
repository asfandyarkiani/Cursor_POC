using Cache.ConfigModels;
using Cache.Interceptor;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cache.Extensions
{
    public static class CachingExtensions
    {
        public static void AddCachedService<TService, TImplementation>(this IServiceCollection services)
                where TService : class
                where TImplementation : class, TService
        {
            services.AddScoped<TService>(sp =>
            {
                // Create the actual instance
                var target = ActivatorUtilities.CreateInstance<TImplementation>(sp);

                // Resolve logger for interceptor
                var logger = sp.GetRequiredService<ILogger<CachingInterceptor>>();

                // Resolve CacheSettings
                var cacheSettings = sp.GetRequiredService<IOptions<CacheSettings>>();

                // Create the caching interceptor
                var asyncInterceptor = new CachingInterceptor(logger, cacheSettings);

                // Wrap with AsyncDeterminationInterceptor
                var interceptor = new AsyncDeterminationInterceptor(asyncInterceptor);

                // Interface proxy
                return new ProxyGenerator()
                    .CreateInterfaceProxyWithTarget<TService>(target, interceptor);
            });
        }
    }
}
