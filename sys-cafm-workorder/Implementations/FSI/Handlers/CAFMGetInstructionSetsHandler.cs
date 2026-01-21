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
/// Handler for CAFM GetInstructionSets operation.
/// Maps API request to downstream request and invokes the atomic handler.
/// </summary>
public class CAFMGetInstructionSetsHandler : IBaseHandler<GetInstructionSetsRequestDto>
{
    private readonly CAFMGetInstructionSetsAtomicHandler _atomicHandler;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<CAFMGetInstructionSetsHandler> _logger;

    public CAFMGetInstructionSetsHandler(
        CAFMGetInstructionSetsAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs,
        ILogger<CAFMGetInstructionSetsHandler> logger)
    {
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs.Value;
        _logger = logger;
    }

    public async Task<BaseResponseDTO> HandleAsync(GetInstructionSetsRequestDto requestDTO)
    {
        _logger.Info($"Processing CAFM GetInstructionSets request for CategoryName: {requestDTO.CategoryName}");

        // Validate request
        requestDTO.ValidateAPIRequestParameters();

        // Map API request to downstream request
        var downstreamRequest = new CAFMGetInstructionSetsDownstreamRequestDto
        {
            SessionId = requestDTO.SessionId,
            CategoryName = requestDTO.CategoryName,
            SubCategory = requestDTO.SubCategory,
            Url = _appConfigs.CAFM.GetFullUrl(),
            SoapAction = _appConfigs.CAFM.SoapActions.GetInstructionSetsByDto
        };

        // Call atomic handler
        var downstreamResponse = await _atomicHandler.Handle(downstreamRequest);

        // Map downstream response to API response
        var apiResponse = new GetInstructionSetsResponseDto
        {
            InstructionSets = downstreamResponse.InstructionSets
        };

        _logger.Info($"CAFM GetInstructionSets completed. Found {apiResponse.TotalCount} instruction sets.");

        return new BaseResponseDTO(
            message: apiResponse.HasInstructionSets 
                ? $"Found {apiResponse.TotalCount} instruction set(s)" 
                : "No instruction sets found",
            errorCode: string.Empty,
            data: apiResponse);
    }
}
