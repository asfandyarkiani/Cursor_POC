using Cache.DTO;
using Cache.Key;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;

namespace Cache.Handlers
{
    public interface ICacheableAtomicHandler<T> : IAtomicHandler<HttpResponseSnapshot> where T : IDownStreamRequestDTO, ICacheable
    {
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(30);

        /// <summary>
        /// Handles the request with caching logic.
        /// If a cached response exists for the request, it is returned.
        /// Otherwise, the request is processed, the response is cached, and then returned.
        /// </summary>
        /// <param name="requestDTO">The request DTO containing the downstream request details.</param>
        /// <returns>
        /// A <see cref="HttpResponseSnapshot"/> representing the cached or freshly retrieved downstream response.
        /// </returns>

        public async Task<HttpResponseSnapshot> HandleCacheAsync(T requestDTO, CacheKeySettings setting)
        {
            CacheKeySettings cacheKeySettings = (setting?.TimeToLive is null || setting.TimeToLive <= TimeSpan.Zero)
                            ? new CacheKeySettings { TimeToLive = DefaultTtl }
                            : setting;

            CacheKey cacheKey = CacheKey.CreateCacheKey(requestDTO.GetCacheKey(), "", cacheKeySettings);
            HttpResponseSnapshot? cacheRes = await cacheKey.GetAsync<HttpResponseSnapshot>();
            if (cacheRes != null)
            {
                return cacheRes;
            }
            Task<HttpResponseSnapshot> result = this.Handle(requestDTO);
            if (result.Result.IsSuccessStatusCode)
            {
                await cacheKey.SetAsync<HttpResponseSnapshot>(result.Result);
            }
            return result.Result;
        }
    }
}
