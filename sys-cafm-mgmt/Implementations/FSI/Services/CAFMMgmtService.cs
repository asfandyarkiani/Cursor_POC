using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using AGI.SystemLayer.CAFM.Abstractions;
using AGI.SystemLayer.CAFM.DTOs.API;
using AGI.SystemLayer.CAFM.Implementations.FSI.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.Services;

/// <summary>
/// Service implementation for CAFM management operations.
/// This service provides the abstraction boundary that delegates to handlers.
/// </summary>
public class CAFMMgmtService : ICAFMMgmt
{
    private readonly CreateWorkOrderHandler _createWorkOrderHandler;
    private readonly ILogger<CAFMMgmtService> _logger;

    public CAFMMgmtService(
        CreateWorkOrderHandler createWorkOrderHandler,
        ILogger<CAFMMgmtService> logger)
    {
        _createWorkOrderHandler = createWorkOrderHandler;
        _logger = logger;
    }

    /// <summary>
    /// Creates a work order in CAFM from an external system
    /// </summary>
    public async Task<CreateWorkOrderResponseDTO> CreateWorkOrderAsync(
        CreateWorkOrderRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CAFMMgmtService: CreateWorkOrder request received");

        // Get session ID from FunctionContext (set by CAFMAuthenticationMiddleware)
        // Note: In a real implementation, you would inject FunctionContext or use a session provider
        // For now, we'll assume the middleware has handled authentication
        var sessionId = GetSessionIdFromContext();

        if (string.IsNullOrEmpty(sessionId))
        {
            _logger.LogError("CAFMMgmtService: No CAFM session ID found in context");
            return new CreateWorkOrderResponseDTO
            {
                Success = false,
                Message = "CAFM session not established. Ensure CAFMAuthenticationAttribute is applied to the function."
            };
        }

        return await _createWorkOrderHandler.CreateWorkOrderAsync(request, sessionId, cancellationToken);
    }

    private string? GetSessionIdFromContext()
    {
        // TODO: Implement proper context retrieval
        // This should be injected via a session provider or context accessor
        // For now, return a placeholder that will be set by middleware
        return "SESSION_ID_FROM_MIDDLEWARE";
    }
}
