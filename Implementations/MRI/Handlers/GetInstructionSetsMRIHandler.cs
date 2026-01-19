using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.DTO.GetInstructionSetsDTO;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetInstructionSetsApiDTO;
using FacilitiesMgmtSystem.Helper;
using FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetInstructionSetsAtomicHandler;

namespace FacilitiesMgmtSystem.Implementations.MRI.Handlers;

/// <summary>
/// MRI Handler for Get Instruction Sets operation.
/// Orchestrates the request processing and data transformation.
/// </summary>
public class GetInstructionSetsMRIHandler
{
    private readonly ILogger<GetInstructionSetsMRIHandler> _logger;
    private readonly GetInstructionSetsAtomicHandler _atomicHandler;
    private readonly IOptions<AppConfigs> _appConfigs;

    public GetInstructionSetsMRIHandler(
        ILogger<GetInstructionSetsMRIHandler> logger,
        GetInstructionSetsAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs;
    }

    /// <summary>
    /// Processes the Get Instruction Sets request.
    /// </summary>
    /// <param name="request">The API request DTO.</param>
    /// <returns>The API response DTO.</returns>
    public async Task<BaseResponseDTO> ProcessRequest(GetInstructionSetsRequestDTO request)
    {
        _logger.Info(InfoConstants.PROCESSING_STARTED, "GetInstructionSets");

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

            _logger.Info(InfoConstants.PROCESSING_COMPLETED, "GetInstructionSets");
            return response;
        }
        catch (DownStreamApiFailureException)
        {
            throw; // Re-throw downstream exceptions
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing GetInstructionSets request.");
            throw new BaseException(ErrorConstants.GET_INSTRUCTION_SETS_FAILED, ex)
            {
                ErrorProperties = [ErrorConstants.GET_INSTRUCTION_SETS_FAILED]
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

    private GetInstructionSetsApiRequestDTO MapToDownstreamRequest(GetInstructionSetsRequestDTO request)
    {
        return new GetInstructionSetsApiRequestDTO
        {
            ContractId = _appConfigs.Value.MRI.ContractId,
            InstructionSetId = request.InstructionSetId,
            CategoryId = request.CategoryId,
            AssetTypeId = request.AssetTypeId,
            IncludeSteps = request.IncludeSteps
        };
    }

    private GetInstructionSetsResponseDTO MapToApiResponse(GetInstructionSetsApiResponseDTO downstreamResponse)
    {
        var result = downstreamResponse.Result;

        if (result == null || !result.Success)
        {
            return new GetInstructionSetsResponseDTO
            {
                Success = false,
                Message = result?.ErrorMessage ?? ErrorConstants.GET_INSTRUCTION_SETS_FAILED,
                ErrorProperties = [result?.ErrorCode ?? ErrorConstants.GET_INSTRUCTION_SETS_FAILED]
            };
        }

        return new GetInstructionSetsResponseDTO
        {
            Success = true,
            Message = result.Message ?? "Instruction sets retrieved successfully.",
            InstructionSets = result.InstructionSets?.Select(MapInstructionSetData).ToList()
        };
    }

    private InstructionSetData MapInstructionSetData(InstructionSetApiDTO apiDto)
    {
        return new InstructionSetData
        {
            InstructionSetId = apiDto.InstructionSetId,
            Name = apiDto.Name,
            Description = apiDto.Description,
            CategoryId = apiDto.CategoryId,
            CategoryName = apiDto.CategoryName,
            AssetTypeId = apiDto.AssetTypeId,
            AssetTypeName = apiDto.AssetTypeName,
            Version = apiDto.Version,
            Status = apiDto.Status,
            EstimatedDurationMinutes = apiDto.EstimatedDurationMinutes,
            RequiredSkillLevel = apiDto.RequiredSkillLevel,
            Steps = apiDto.Steps?.Select(MapStepData).ToList()
        };
    }

    private InstructionStepData MapStepData(InstructionStepApiDTO apiDto)
    {
        return new InstructionStepData
        {
            StepNumber = apiDto.StepNumber,
            Title = apiDto.Title,
            Description = apiDto.Description,
            EstimatedMinutes = apiDto.EstimatedMinutes,
            SafetyNotes = apiDto.SafetyNotes,
            RequiredTools = apiDto.RequiredTools,
            IsMandatory = apiDto.IsMandatory
        };
    }
}
