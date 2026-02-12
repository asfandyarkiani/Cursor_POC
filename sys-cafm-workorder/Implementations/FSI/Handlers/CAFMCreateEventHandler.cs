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
/// Handler for CAFM CreateEvent operation.
/// Maps API request to downstream request and invokes the atomic handler.
/// </summary>
public class CAFMCreateEventHandler : IBaseHandler<CreateEventRequestDto>
{
    private readonly CAFMCreateEventAtomicHandler _atomicHandler;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<CAFMCreateEventHandler> _logger;

    public CAFMCreateEventHandler(
        CAFMCreateEventAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs,
        ILogger<CAFMCreateEventHandler> logger)
    {
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs.Value;
        _logger = logger;
    }

    public async Task<BaseResponseDTO> HandleAsync(CreateEventRequestDto requestDTO)
    {
        _logger.Info($"Processing CAFM CreateEvent request for TaskId: {requestDTO.TaskId}");

        // Validate request
        requestDTO.ValidateAPIRequestParameters();

        // Map API request to downstream request
        var downstreamRequest = new CAFMCreateEventDownstreamRequestDto
        {
            SessionId = requestDTO.SessionId,
            TaskId = requestDTO.TaskId,
            EventType = requestDTO.EventType,
            Description = requestDTO.Description,
            EventDateUtc = CAFMCreateEventDownstreamRequestDto.FormatDateForCAFM(requestDTO.EventDateUtc),
            Url = _appConfigs.CAFM.GetFullUrl(),
            SoapAction = _appConfigs.CAFM.SoapActions.CreateEvent
        };

        // Call atomic handler
        var downstreamResponse = await _atomicHandler.Handle(downstreamRequest);

        // Map downstream response to API response
        var apiResponse = new CreateEventResponseDto
        {
            TaskId = requestDTO.TaskId,
            IsCreated = downstreamResponse.IsSuccess
        };

        _logger.Info($"CAFM CreateEvent completed. Success: {apiResponse.IsCreated}");

        return new BaseResponseDTO(
            message: "Event created successfully",
            errorCode: string.Empty,
            data: apiResponse);
    }
}
