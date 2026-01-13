using SystemLayer.Application.DTOs;

namespace SystemLayer.Application.Interfaces;

/// <summary>
/// Interface for CAFM integration operations
/// </summary>
public interface ICafmService
{
    /// <summary>
    /// Creates a work order in CAFM system
    /// </summary>
    /// <param name="request">Work order creation request</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Work order creation result</returns>
    Task<SystemLayerResult<CreateWorkOrderResponse>> CreateWorkOrderAsync(
        CreateWorkOrderRequest request, 
        string correlationId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves location information from CAFM system
    /// </summary>
    /// <param name="request">Location retrieval request</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Location information result</returns>
    Task<SystemLayerResult<LocationResponse>> GetLocationAsync(
        GetLocationRequest request, 
        string correlationId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves breakdown task information from CAFM system
    /// </summary>
    /// <param name="request">Breakdown task retrieval request</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Breakdown task information result</returns>
    Task<SystemLayerResult<BreakdownTaskResponse>> GetBreakdownTaskAsync(
        GetBreakdownTaskRequest request, 
        string correlationId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves instruction sets from CAFM system
    /// </summary>
    /// <param name="request">Instruction sets retrieval request</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Instruction sets result</returns>
    Task<SystemLayerResult<GetInstructionSetsResponse>> GetInstructionSetsAsync(
        GetInstructionSetsRequest request, 
        string correlationId, 
        CancellationToken cancellationToken = default);
}