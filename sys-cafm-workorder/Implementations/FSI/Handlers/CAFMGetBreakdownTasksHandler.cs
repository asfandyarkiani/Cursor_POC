using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sys_cafm_workorder.ConfigModels;
using sys_cafm_workorder.DTOs.Api.CAFM;
using sys_cafm_workorder.DTOs.Downstream.CAFM;
using sys_cafm_workorder.Implementations.FSI.AtomicHandlers;

namespace sys_cafm_workorder.Implementations.FSI.Handlers;

/// <summary>
/// Handler for CAFM GetBreakdownTasks operation.
/// Maps API request to downstream request and invokes the atomic handler.
/// </summary>
public class CAFMGetBreakdownTasksHandler : IBaseHandler<GetBreakdownTasksRequestDto>
{
    private readonly CAFMGetBreakdownTasksAtomicHandler _atomicHandler;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<CAFMGetBreakdownTasksHandler> _logger;

    public CAFMGetBreakdownTasksHandler(
        CAFMGetBreakdownTasksAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs,
        ILogger<CAFMGetBreakdownTasksHandler> logger)
    {
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs.Value;
        _logger = logger;
    }

    public async Task<BaseResponseDTO> HandleAsync(GetBreakdownTasksRequestDto requestDTO)
    {
        _logger.Info($"Processing CAFM GetBreakdownTasks request for CallerSourceId: {requestDTO.CallerSourceId}");

        // Validate request
        requestDTO.ValidateAPIRequestParameters();

        // Map API request to downstream request
        var downstreamRequest = new CAFMGetBreakdownTasksDownstreamRequestDto
        {
            SessionId = requestDTO.SessionId,
            CallerSourceId = requestDTO.CallerSourceId,
            Url = _appConfigs.CAFM.GetFullUrl(),
            SoapAction = _appConfigs.CAFM.SoapActions.GetBreakdownTasksByDto
        };

        // Call atomic handler
        var downstreamResponse = await _atomicHandler.Handle(downstreamRequest);

        // Map downstream response to API response
        var apiResponse = new GetBreakdownTasksResponseDto
        {
            Tasks = downstreamResponse.Tasks
        };

        _logger.Info($"CAFM GetBreakdownTasks completed. Found {apiResponse.TotalCount} tasks.");

        return new BaseResponseDTO(
            message: apiResponse.HasTasks 
                ? $"Found {apiResponse.TotalCount} breakdown task(s)" 
                : "No breakdown tasks found",
            errorCode: string.Empty,
            data: apiResponse);
    }
}
