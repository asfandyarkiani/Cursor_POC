using Core.SystemLayer.DTOs;

namespace Core.SystemLayer.Handlers
{
    public interface IAtomicHandler<T>
    {
        /// <summary>
        // Will orchestrate Validate, Map, and Actual downstream call
        /// </summary>
        Task<T> Handle(IDownStreamRequestDTO downStreamRequestDTO);
    }
}
