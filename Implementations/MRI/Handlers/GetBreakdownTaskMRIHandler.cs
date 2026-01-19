using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.DTO.GetBreakdownTaskDTO;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetBreakdownTaskApiDTO;
using FacilitiesMgmtSystem.Helper;
using FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetBreakdownTaskAtomicHandler;

namespace FacilitiesMgmtSystem.Implementations.MRI.Handlers;

/// <summary>
/// MRI Handler for Get Breakdown Task operation.
/// Orchestrates the request processing and data transformation.
/// </summary>
public class GetBreakdownTaskMRIHandler
{
    private readonly ILogger<GetBreakdownTaskMRIHandler> _logger;
    private readonly GetBreakdownTaskAtomicHandler _atomicHandler;
    private readonly IOptions<AppConfigs> _appConfigs;

    public GetBreakdownTaskMRIHandler(
        ILogger<GetBreakdownTaskMRIHandler> logger,
        GetBreakdownTaskAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs;
    }

    /// <summary>
    /// Processes the Get Breakdown Task request.
    /// </summary>
    /// <param name="request">The API request DTO.</param>
    /// <returns>The API response DTO.</returns>
    public async Task<BaseResponseDTO> ProcessRequest(GetBreakdownTaskRequestDTO request)
    {
        _logger.Info(InfoConstants.PROCESSING_STARTED, "GetBreakdownTask");

        try
        {
            // Validate session
            ValidateSession(request.SessionId);

            // Map API request to downstream request
            var downstreamRequest = MapToDownstreamRequest(request);

            // Call atomic handler
            var downstreamResponse = await _atomicHandler.ExecuteAsync(
                request.SessionId!,
                downstreamRequest);

            // Map downstream response to API response
            var response = MapToApiResponse(downstreamResponse);

            _logger.Info(InfoConstants.PROCESSING_COMPLETED, "GetBreakdownTask");
            return response;
        }
        catch (DownStreamApiFailureException)
        {
            throw; // Re-throw downstream exceptions
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing GetBreakdownTask request.");
            throw new BaseException(ErrorConstants.GET_BREAKDOWN_TASK_FAILED, ex)
            {
                ErrorProperties = [ErrorConstants.GET_BREAKDOWN_TASK_FAILED]
            };
        }
    }

    private void ValidateSession(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new BaseException(ErrorConstants.SESSION_ID_NOT_FOUND_IN_CONTEXT)
            {
                ErrorProperties = [ErrorConstants.MRI_AUTHENTICATION_REQUIRED]
            };
        }
    }

    private GetBreakdownTaskApiRequestDTO MapToDownstreamRequest(GetBreakdownTaskRequestDTO request)
    {
        return new GetBreakdownTaskApiRequestDTO
        {
            ContractId = _appConfigs.Value.MRI.ContractId,
            TaskId = request.TaskId,
            WorkOrderId = request.WorkOrderId,
            IncludeDetails = request.IncludeDetails
        };
    }

    private GetBreakdownTaskResponseDTO MapToApiResponse(GetBreakdownTaskApiResponseDTO downstreamResponse)
    {
        var result = downstreamResponse.Result;

        if (result == null || !result.Success)
        {
            return new GetBreakdownTaskResponseDTO
            {
                Success = false,
                Message = result?.ErrorMessage ?? ErrorConstants.GET_BREAKDOWN_TASK_FAILED,
                ErrorProperties = [result?.ErrorCode ?? ErrorConstants.GET_BREAKDOWN_TASK_FAILED]
            };
        }

        return new GetBreakdownTaskResponseDTO
        {
            Success = true,
            Message = result.Message ?? "Breakdown tasks retrieved successfully.",
            Tasks = result.Tasks?.Select(MapTaskData).ToList()
        };
    }

    private BreakdownTaskData MapTaskData(BreakdownTaskApiDTO apiDto)
    {
        return new BreakdownTaskData
        {
            TaskId = apiDto.TaskId,
            TaskName = apiDto.TaskName,
            Description = apiDto.Description,
            WorkOrderId = apiDto.WorkOrderId,
            Status = apiDto.Status,
            Priority = apiDto.Priority,
            EstimatedHours = apiDto.EstimatedHours,
            ActualHours = apiDto.ActualHours,
            AssignedTo = apiDto.AssignedTo,
            ScheduledStartDate = apiDto.ScheduledStartDate,
            ScheduledEndDate = apiDto.ScheduledEndDate,
            SequenceNumber = apiDto.SequenceNumber,
            InstructionSetId = apiDto.InstructionSetId
        };
    }
}
