using Core.DTOs;
using Core.SystemLayer.DTOs;

namespace Core.SystemLayer.Handlers
{
    public interface IBaseHandler<T> where T : IRequestSysDTO
    {
        /// <summary>
        /// Handles the orchestration logic for the given request DTO and returns a base response.
        /// </summary>
        Task<BaseResponseDTO> HandleAsync(T requestDTO);
    }
}
