using CAFMSystem.DTO.CreateBreakdownTaskDTO;
using CAFMSystem.DTO.CreateEventDTO;
using CAFMSystem.DTO.GetBreakdownTasksByDtoDTO;
using Core.DTOs;
using System.Threading.Tasks;

namespace CAFMSystem.Abstractions
{
    /// <summary>
    /// Interface for CAFM Breakdown Task management operations
    /// </summary>
    public interface IBreakdownTaskMgmt
    {
        /// <summary>
        /// Get breakdown tasks by service request number (check if exists)
        /// </summary>
        Task<BaseResponseDTO> GetBreakdownTasksByDto(GetBreakdownTasksByDtoReqDTO request);
        
        /// <summary>
        /// Create breakdown task (work order) in CAFM
        /// </summary>
        Task<BaseResponseDTO> CreateBreakdownTask(CreateBreakdownTaskReqDTO request);
        
        /// <summary>
        /// Create recurring event for breakdown task
        /// </summary>
        Task<BaseResponseDTO> CreateEvent(CreateEventReqDTO request);
    }
}
