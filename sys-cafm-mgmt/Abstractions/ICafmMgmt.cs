using Core.DTOs;
using SysCafmMgmt.DTOs.API;

namespace SysCafmMgmt.Abstractions
{
    /// <summary>
    /// Interface for CAFM (Computer-Aided Facility Management) operations
    /// Provides methods to interact with FSI CAFM system for work order management
    /// </summary>
    public interface ICafmMgmt
    {
        /// <summary>
        /// Authenticates with CAFM system and obtains a session ID
        /// </summary>
        Task<BaseResponseDTO> LoginAsync();

        /// <summary>
        /// Terminates the session with CAFM system
        /// </summary>
        Task<BaseResponseDTO> LogoutAsync(string sessionId);

        /// <summary>
        /// Creates a work order in CAFM system
        /// </summary>
        Task<BaseResponseDTO> CreateWorkOrderAsync(CreateWorkOrderRequestDTO request);

        /// <summary>
        /// Retrieves locations from CAFM system based on search criteria
        /// </summary>
        Task<BaseResponseDTO> GetLocationsAsync(GetLocationsRequestDTO request);

        /// <summary>
        /// Retrieves instruction sets from CAFM system
        /// </summary>
        Task<BaseResponseDTO> GetInstructionSetsAsync(GetInstructionSetsRequestDTO request);

        /// <summary>
        /// Retrieves breakdown tasks from CAFM system
        /// </summary>
        Task<BaseResponseDTO> GetBreakdownTasksAsync(GetBreakdownTasksRequestDTO request);

        /// <summary>
        /// Creates an event/links a task in CAFM system
        /// </summary>
        Task<BaseResponseDTO> CreateEventAsync(CreateEventRequestDTO request);
    }
}
