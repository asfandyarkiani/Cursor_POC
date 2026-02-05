using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.DTOs.API;
using SysCafmMgmt.Implementations.FSI.Handlers;

namespace SysCafmMgmt.Implementations.FSI.Services;

/// <summary>
/// FSI CAFM implementation of Work Order Management
/// This service acts as an abstraction boundary that delegates to handlers
/// </summary>
public class WorkOrderService : IWorkOrderMgmt
{
    private readonly CreateWorkOrderHandler _createWorkOrderHandler;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        CreateWorkOrderHandler createWorkOrderHandler,
        ILogger<WorkOrderService> logger)
    {
        _createWorkOrderHandler = createWorkOrderHandler;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> CreateWorkOrderAsync(CreateWorkOrderRequestDTO request)
    {
        _logger.Info("Starting CreateWorkOrderAsync");
        
        // Validate request
        request.ValidateAPIRequestParameters();

        // Delegate to handler
        return await _createWorkOrderHandler.HandleAsync(request);
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> GetWorkOrderAsync(string taskNumber)
    {
        _logger.Info($"GetWorkOrderAsync for task: {taskNumber}");
        
        // TODO: Implement GetWorkOrder functionality
        // This would require GetBreakdownTasksByDto atomic handler
        throw new NotImplementedException("GetWorkOrder not yet implemented");
    }
}
