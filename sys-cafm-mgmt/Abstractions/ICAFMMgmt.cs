using AGI.SystemLayer.CAFM.DTOs.API;

namespace AGI.SystemLayer.CAFM.Abstractions;

/// <summary>
/// Interface for CAFM (Computer-Aided Facility Management) System Layer operations.
/// This interface declares all operations needed to interact with the CAFM system of record.
/// </summary>
public interface ICAFMMgmt
{
    /// <summary>
    /// Creates a work order in CAFM from an external system (e.g., EQ+)
    /// </summary>
    /// <param name="request">Work order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Work order creation response with CAFM-generated task ID</returns>
    Task<CreateWorkOrderResponseDTO> CreateWorkOrderAsync(
        CreateWorkOrderRequestDTO request,
        CancellationToken cancellationToken = default);
}
