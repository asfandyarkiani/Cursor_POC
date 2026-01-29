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
/// Handler for CAFM GetLocations operation.
/// Maps API request to downstream request and invokes the atomic handler.
/// </summary>
public class CAFMGetLocationsHandler : IBaseHandler<GetLocationsRequestDto>
{
    private readonly CAFMGetLocationsAtomicHandler _atomicHandler;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<CAFMGetLocationsHandler> _logger;

    public CAFMGetLocationsHandler(
        CAFMGetLocationsAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs,
        ILogger<CAFMGetLocationsHandler> logger)
    {
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs.Value;
        _logger = logger;
    }

    public async Task<BaseResponseDTO> HandleAsync(GetLocationsRequestDto requestDTO)
    {
        _logger.Info($"Processing CAFM GetLocations request for PropertyName: {requestDTO.PropertyName}");

        // Validate request
        requestDTO.ValidateAPIRequestParameters();

        // Map API request to downstream request
        var downstreamRequest = new CAFMGetLocationsDownstreamRequestDto
        {
            SessionId = requestDTO.SessionId,
            PropertyName = requestDTO.PropertyName,
            UnitCode = requestDTO.UnitCode,
            Url = _appConfigs.CAFM.GetFullUrl(),
            SoapAction = _appConfigs.CAFM.SoapActions.GetLocationsByDto
        };

        // Call atomic handler
        var downstreamResponse = await _atomicHandler.Handle(downstreamRequest);

        // Map downstream response to API response
        var apiResponse = new GetLocationsResponseDto
        {
            Locations = downstreamResponse.Locations
        };

        _logger.Info($"CAFM GetLocations completed. Found {apiResponse.TotalCount} locations.");

        return new BaseResponseDTO(
            message: apiResponse.HasLocations 
                ? $"Found {apiResponse.TotalCount} location(s)" 
                : "No locations found",
            errorCode: string.Empty,
            data: apiResponse);
    }
}
