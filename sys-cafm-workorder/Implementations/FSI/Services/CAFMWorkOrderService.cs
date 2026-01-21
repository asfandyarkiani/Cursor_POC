using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.Abstractions;
using sys_cafm_workorder.DTOs.Api.CAFM;
using sys_cafm_workorder.Implementations.FSI.Handlers;

namespace sys_cafm_workorder.Implementations.FSI.Services;

/// <summary>
/// Service implementation for CAFM Work Order operations.
/// Provides a clean interface for work order-related operations.
/// </summary>
public class CAFMWorkOrderService : ICAFMWorkOrderService
{
    private readonly CAFMGetBreakdownTasksHandler _getBreakdownTasksHandler;
    private readonly CAFMGetLocationsHandler _getLocationsHandler;
    private readonly CAFMGetInstructionSetsHandler _getInstructionSetsHandler;
    private readonly CAFMCreateBreakdownTaskHandler _createBreakdownTaskHandler;
    private readonly CAFMCreateEventHandler _createEventHandler;
    private readonly ILogger<CAFMWorkOrderService> _logger;

    public CAFMWorkOrderService(
        CAFMGetBreakdownTasksHandler getBreakdownTasksHandler,
        CAFMGetLocationsHandler getLocationsHandler,
        CAFMGetInstructionSetsHandler getInstructionSetsHandler,
        CAFMCreateBreakdownTaskHandler createBreakdownTaskHandler,
        CAFMCreateEventHandler createEventHandler,
        ILogger<CAFMWorkOrderService> logger)
    {
        _getBreakdownTasksHandler = getBreakdownTasksHandler;
        _getLocationsHandler = getLocationsHandler;
        _getInstructionSetsHandler = getInstructionSetsHandler;
        _createBreakdownTaskHandler = createBreakdownTaskHandler;
        _createEventHandler = createEventHandler;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> GetBreakdownTasksAsync(GetBreakdownTasksRequestDto request)
    {
        _logger.Info("CAFMWorkOrderService: Processing GetBreakdownTasks request");
        return await _getBreakdownTasksHandler.HandleAsync(request);
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> GetLocationsAsync(GetLocationsRequestDto request)
    {
        _logger.Info("CAFMWorkOrderService: Processing GetLocations request");
        return await _getLocationsHandler.HandleAsync(request);
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> GetInstructionSetsAsync(GetInstructionSetsRequestDto request)
    {
        _logger.Info("CAFMWorkOrderService: Processing GetInstructionSets request");
        return await _getInstructionSetsHandler.HandleAsync(request);
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> CreateBreakdownTaskAsync(CreateBreakdownTaskRequestDto request)
    {
        _logger.Info("CAFMWorkOrderService: Processing CreateBreakdownTask request");
        return await _createBreakdownTaskHandler.HandleAsync(request);
    }

    /// <inheritdoc/>
    public async Task<BaseResponseDTO> CreateEventAsync(CreateEventRequestDto request)
    {
        _logger.Info("CAFMWorkOrderService: Processing CreateEvent request");
        return await _createEventHandler.HandleAsync(request);
    }
}
