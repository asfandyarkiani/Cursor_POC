using Cache.DTO;
using Cache.Key;
using Core.DTOs;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;

namespace Cache.Handlers
{
    public interface ICacheableHandler<T> : IBaseHandler<T> where T : IRequestSysDTO, ICacheable
    {

        /// <summary>
        /// Handles the request with caching logic.
        /// If a cached <see cref="BaseResponseDTO"/> exists for the given request, it is returned.
        /// Otherwise, the request is processed, the response is cached if it has no errors, and then returned.
        /// </summary>
        /// <param name="requestDTO">The request DTO containing the request details.</param>
        /// <returns>
        /// A <see cref="BaseResponseDTO"/> representing the cached or freshly processed result.
        /// </returns>
        
        public async Task<BaseResponseDTO> HandleCacheAsync(T requestDTO)
        {
            CacheKeySettings cacheKeySettings = new CacheKeySettings { TimeToLive = TimeSpan.FromMinutes(5) };
            CacheKey cacheKey = CacheKey.CreateCacheKey(requestDTO.GetCacheKey(), "", cacheKeySettings);
            BaseResponseDTO? cacheRes = await cacheKey.GetAsync<BaseResponseDTO>();
            if (cacheRes != null)
            {
                return cacheRes;
            }

            Task<BaseResponseDTO> result = this.HandleAsync(requestDTO);
            if (result.Result?.ErrorDetails?.Errors == null || result.Result.ErrorDetails.Errors.Count == 0)
            {
                await cacheKey.SetAsync<BaseResponseDTO>(result.Result!);
            }

            return result.Result;
        }
    }
}
