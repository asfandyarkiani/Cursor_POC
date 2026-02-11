using Core.DTOs;
using SysCafmMgmt.DTOs.API;

namespace SysCafmMgmt.Abstractions;

/// <summary>
/// Interface for Work Order Management operations
/// Implementations: FSI CAFM
/// </summary>
public interface IWorkOrderMgmt
{
    /// <summary>
    /// Creates a new work order in the CAFM system
    /// </summary>
    /// <param name="request">Work order creation request</param>
    /// <returns>Base response with work order details</returns>
    Task<BaseResponseDTO> CreateWorkOrderAsync(CreateWorkOrderRequestDTO request);

    /// <summary>
    /// Gets work order details by task number
    /// </summary>
    /// <param name="taskNumber">Task number to search for</param>
    /// <returns>Base response with work order details</returns>
    Task<BaseResponseDTO> GetWorkOrderAsync(string taskNumber);
}
