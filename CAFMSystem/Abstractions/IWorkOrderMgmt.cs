using CAFMSystem.DTO.CreateBreakdownTaskDTO;
using CAFMSystem.DTO.CreateEventDTO;
using CAFMSystem.DTO.GetBreakdownTasksByDtoDTO;
using Core.DTOs;
using System.Threading.Tasks;

namespace CAFMSystem.Abstractions
{
    /// <summary>
    /// Interface for Work Order management operations in CAFM system.
    /// Defines contracts for breakdown task and event operations.
    /// </summary>
    public interface IWorkOrderMgmt
    {
        /// <summary>
        /// Checks if breakdown task already exists in CAFM based on service request number.
        /// </summary>
        Task<BaseResponseDTO> GetBreakdownTasksByDto(GetBreakdownTasksByDtoReqDTO request);

        /// <summary>
        /// Creates a new breakdown task/work order in CAFM system.
        /// Orchestrates lookups (locations, instructions) and creates task.
        /// </summary>
        Task<BaseResponseDTO> CreateBreakdownTask(CreateBreakdownTaskReqDTO request);

        /// <summary>
        /// Creates and links a recurring event to an existing breakdown task.
        /// </summary>
        Task<BaseResponseDTO> CreateEvent(CreateEventReqDTO request);
    }
}
