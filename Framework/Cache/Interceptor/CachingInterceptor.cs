using Cache.Attributes;
using Cache.ConfigModels;
using Cache.DTO;
using Cache.Key;
using Castle.DynamicProxy;
using Core.Extensions;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Cache.Interceptor
{
    public class CachingInterceptor : IAsyncInterceptor
    {
        private readonly ILogger<CachingInterceptor> _logger;
        private readonly CacheSettings _cacheSettings;
        private static readonly AsyncLocal<bool> _isIntercepting = new();

        public CachingInterceptor(ILogger<CachingInterceptor> logger, IOptions<CacheSettings> cacheSettings)
        {
            _logger = logger;
            _cacheSettings = cacheSettings.Value;
        }
        public void InterceptSynchronous(IInvocation invocation)
        {
            invocation.Proceed();
        }
        public void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InterceptAsync(invocation);
        }
        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InterceptAsync<TResult>(invocation);
        }
        private async Task InterceptAsync(IInvocation invocation)
        {
            if (_isIntercepting.Value)
            {
                invocation.Proceed();
                await (Task)invocation.ReturnValue!;
                return;
            }
            _isIntercepting.Value = true;
            try
            {
                var method = invocation.MethodInvocationTarget ?? invocation.Method;
                var cacheAttr = method.GetCustomAttribute<CacheableServiceAttribute>();
                if (cacheAttr == null)
                {
                    invocation.Proceed();
                    await (Task)invocation.ReturnValue!;
                    return;
                }
                var dto = invocation.Arguments.FirstOrDefault(a => a is ICacheable);
                if (dto is not ICacheable cacheable)
                {
                    invocation.Proceed();
                    await (Task)invocation.ReturnValue!;
                    return;
                }

                // Get TTL from configuration based on method name
                int ttlMinutes = _cacheSettings.GetTTLForMethod(method.Name);

                CacheKeySettings cacheKeySettings = new CacheKeySettings
                {
                    TimeToLive = TimeSpan.FromMinutes(ttlMinutes)
                };

                string ns = cacheAttr.Namespace ?? string.Empty;
                CacheKey cacheKey = CacheKey.CreateCacheKey(cacheable.GetCacheKey(), ns, cacheKeySettings);
                var cachedValue = await cacheKey.GetAsync<object>();
                if (cachedValue != null)
                {
                    _logger.Info($"Cache HIT for {method.Name} with key {cacheKey.InternalKey.ToString()} (TTL: {ttlMinutes}m)");
                    invocation.ReturnValue = Task.FromResult(cachedValue);
                    return;
                }
                _logger.Info($"Cache MISS for {method.Name} with key {cacheKey.InternalKey.ToString()} (TTL: {ttlMinutes}m)");
                invocation.Proceed();
                await (Task)invocation.ReturnValue!;
                await cacheKey.SetAsync(invocation.ReturnValue);
            }
            finally
            {
                _isIntercepting.Value = false;
            }
        }
        private async Task<TResult> InterceptAsync<TResult>(IInvocation invocation)
        {
            if (_isIntercepting.Value)
            {
                invocation.Proceed();
                return await (Task<TResult>)invocation.ReturnValue!;
            }
            _isIntercepting.Value = true;
            try
            {
                var method = invocation.MethodInvocationTarget ?? invocation.Method;
                var cacheAttr = method.GetCustomAttribute<CacheableServiceAttribute>();
                if (cacheAttr == null)
                {
                    invocation.Proceed();
                    return await (Task<TResult>)invocation.ReturnValue!;
                }
                var dto = invocation.Arguments.FirstOrDefault(a => a is ICacheable);
                if (dto is not ICacheable cacheable)
                {
                    invocation.Proceed();
                    return await (Task<TResult>)invocation.ReturnValue!;
                }

                // Get TTL from configuration based on method name
                int ttlMinutes = _cacheSettings.GetTTLForMethod(method.Name);

                CacheKeySettings cacheKeySettings = new CacheKeySettings
                {
                    TimeToLive = TimeSpan.FromMinutes(ttlMinutes)
                };
                string ns = cacheAttr.Namespace ?? string.Empty;
                CacheKey cacheKey = CacheKey.CreateCacheKey(cacheable.GetCacheKey(), ns, cacheKeySettings);
                var cachedValue = await cacheKey.GetAsync<TResult>();
                if (cachedValue != null)
                {
                    _logger.Info($"Cache HIT for {method.Name} with key {cacheKey.InternalKey.ToString()} (TTL: {ttlMinutes}m)");
                    return cachedValue;
                }
                _logger.Info($"Cache MISS for {method.Name} with key {cacheKey.InternalKey.ToString()} (TTL: {ttlMinutes}m)");
                invocation.Proceed();
                var result = await (Task<TResult>)invocation.ReturnValue!;

                if (result is HttpResponseSnapshot snapshot)
                {
                    if (snapshot.IsSuccessStatusCode)
                    {
                        await cacheKey.SetAsync(result);
                        _logger.Info($"Cache SET for {method.Name} with key {cacheKey.InternalKey.ToString()} (TTL: {ttlMinutes}m, success)");
                    }
                }

                return result;
            }
            finally
            {
                _isIntercepting.Value = false;
            }
        }
    }
}
