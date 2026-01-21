using Core.DTOs;
using sys_cafm_workorder.DTOs.Api.CAFM;

namespace sys_cafm_workorder.Abstractions;

/// <summary>
/// Interface for CAFM Work Order operations.
/// Handles all work order related operations in the CAFM (FSI) system.
/// </summary>
public interface ICAFMWorkOrderService
{
    /// <summary>
    /// Gets breakdown tasks from CAFM based on caller source ID.
    /// </summary>
    /// <param name="request">Request containing session ID and filter criteria.</param>
    /// <returns>Response containing matching breakdown tasks.</returns>
    Task<BaseResponseDTO> GetBreakdownTasksAsync(GetBreakdownTasksRequestDto request);

    /// <summary>
    /// Gets locations from CAFM based on property name and optional unit code.
    /// </summary>
    /// <param name="request">Request containing session ID and location search criteria.</param>
    /// <returns>Response containing matching locations.</returns>
    Task<BaseResponseDTO> GetLocationsAsync(GetLocationsRequestDto request);

    /// <summary>
    /// Gets instruction sets from CAFM based on category and optional sub-category.
    /// </summary>
    /// <param name="request">Request containing session ID and instruction set search criteria.</param>
    /// <returns>Response containing matching instruction sets.</returns>
    Task<BaseResponseDTO> GetInstructionSetsAsync(GetInstructionSetsRequestDto request);

    /// <summary>
    /// Creates a breakdown task in CAFM.
    /// </summary>
    /// <param name="request">Request containing all breakdown task details.</param>
    /// <returns>Response containing the created task ID.</returns>
    Task<BaseResponseDTO> CreateBreakdownTaskAsync(CreateBreakdownTaskRequestDto request);

    /// <summary>
    /// Creates an event (links a task) in CAFM.
    /// </summary>
    /// <param name="request">Request containing event details and task to link.</param>
    /// <returns>Response indicating event creation success/failure.</returns>
    Task<BaseResponseDTO> CreateEventAsync(CreateEventRequestDto request);
}
