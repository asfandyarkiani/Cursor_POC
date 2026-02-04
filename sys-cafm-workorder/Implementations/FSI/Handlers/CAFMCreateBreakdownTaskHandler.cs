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
/// Handler for CAFM CreateBreakdownTask operation.
/// Maps API request to downstream request and invokes the atomic handler.
/// </summary>
public class CAFMCreateBreakdownTaskHandler : IBaseHandler<CreateBreakdownTaskRequestDto>
{
    private readonly CAFMCreateBreakdownTaskAtomicHandler _atomicHandler;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<CAFMCreateBreakdownTaskHandler> _logger;

    public CAFMCreateBreakdownTaskHandler(
        CAFMCreateBreakdownTaskAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs,
        ILogger<CAFMCreateBreakdownTaskHandler> logger)
    {
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs.Value;
        _logger = logger;
    }

    public async Task<BaseResponseDTO> HandleAsync(CreateBreakdownTaskRequestDto requestDTO)
    {
        _logger.Info($"Processing CAFM CreateBreakdownTask request for CallerSourceId: {requestDTO.CallerSourceId}");

        // Validate request
        requestDTO.ValidateAPIRequestParameters();

        // Map API request to downstream request
        var downstreamRequest = new CAFMCreateBreakdownTaskDownstreamRequestDto
        {
            SessionId = requestDTO.SessionId,
            BuildingId = requestDTO.BuildingId,
            LocationId = requestDTO.LocationId,
            CategoryId = requestDTO.CategoryId,
            InstructionId = requestDTO.InstructionId,
            DisciplineId = requestDTO.DisciplineId,
            PriorityId = requestDTO.PriorityId,
            ContractId = string.IsNullOrWhiteSpace(requestDTO.ContractId) 
                ? _appConfigs.CAFM.ContractId 
                : requestDTO.ContractId,
            CallerSourceId = requestDTO.CallerSourceId,
            ReporterName = requestDTO.ReporterName,
            ReporterEmail = requestDTO.ReporterEmail,
            ReporterPhone = requestDTO.ReporterPhone,
            Description = requestDTO.Description,
            RaisedDateUtc = CAFMCreateBreakdownTaskDownstreamRequestDto.FormatDateForCAFM(requestDTO.RaisedDateUtc),
            ScheduledDateUtc = CAFMCreateBreakdownTaskDownstreamRequestDto.FormatDateForCAFM(requestDTO.ScheduledDateUtc),
            Url = _appConfigs.CAFM.GetFullUrl(),
            SoapAction = _appConfigs.CAFM.SoapActions.CreateBreakdownTask
        };

        // Call atomic handler
        var downstreamResponse = await _atomicHandler.Handle(downstreamRequest);

        // Map downstream response to API response
        var apiResponse = new CreateBreakdownTaskResponseDto
        {
            TaskId = downstreamResponse.TaskId,
            IsCreated = downstreamResponse.IsSuccess,
            CallerSourceId = requestDTO.CallerSourceId
        };

        _logger.Info($"CAFM CreateBreakdownTask completed. TaskId: {apiResponse.TaskId}");

        return new BaseResponseDTO(
            message: "Breakdown task created successfully",
            errorCode: string.Empty,
            data: apiResponse);
    }
}
