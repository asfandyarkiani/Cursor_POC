using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.DTO.GetLocationDTO;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetLocationApiDTO;
using FacilitiesMgmtSystem.Helper;
using FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetLocationAtomicHandler;

namespace FacilitiesMgmtSystem.Implementations.MRI.Handlers;

/// <summary>
/// MRI Handler for Get Location operation.
/// Orchestrates the request processing and data transformation.
/// </summary>
public class GetLocationMRIHandler
{
    private readonly ILogger<GetLocationMRIHandler> _logger;
    private readonly GetLocationAtomicHandler _atomicHandler;
    private readonly IOptions<AppConfigs> _appConfigs;

    public GetLocationMRIHandler(
        ILogger<GetLocationMRIHandler> logger,
        GetLocationAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs;
    }

    /// <summary>
    /// Processes the Get Location request.
    /// </summary>
    /// <param name="request">The API request DTO.</param>
    /// <returns>The API response DTO.</returns>
    public async Task<BaseResponseDTO> ProcessRequest(GetLocationRequestDTO request)
    {
        _logger.Info(InfoConstants.PROCESSING_STARTED, "GetLocation");

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

            _logger.Info(InfoConstants.PROCESSING_COMPLETED, "GetLocation");
            return response;
        }
        catch (DownStreamApiFailureException)
        {
            throw; // Re-throw downstream exceptions
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing GetLocation request.");
            throw new BaseException(ErrorConstants.GET_LOCATION_FAILED, ex)
            {
                ErrorProperties = [ErrorConstants.GET_LOCATION_FAILED]
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

    private GetLocationApiRequestDTO MapToDownstreamRequest(GetLocationRequestDTO request)
    {
        return new GetLocationApiRequestDTO
        {
            ContractId = _appConfigs.Value.MRI.ContractId,
            LocationId = request.LocationId,
            LocationCode = request.LocationCode,
            BuildingId = request.BuildingId,
            FloorId = request.FloorId,
            IncludeHierarchy = request.IncludeHierarchy
        };
    }

    private GetLocationResponseDTO MapToApiResponse(GetLocationApiResponseDTO downstreamResponse)
    {
        var result = downstreamResponse.Result;

        if (result == null || !result.Success)
        {
            return new GetLocationResponseDTO
            {
                Success = false,
                Message = result?.ErrorMessage ?? ErrorConstants.GET_LOCATION_FAILED,
                ErrorProperties = [result?.ErrorCode ?? ErrorConstants.GET_LOCATION_FAILED]
            };
        }

        return new GetLocationResponseDTO
        {
            Success = true,
            Message = result.Message ?? "Locations retrieved successfully.",
            Locations = result.Locations?.Select(MapLocationData).ToList()
        };
    }

    private LocationData MapLocationData(LocationApiDTO apiDto)
    {
        return new LocationData
        {
            LocationId = apiDto.LocationId,
            LocationCode = apiDto.LocationCode,
            Name = apiDto.Name,
            Description = apiDto.Description,
            LocationType = apiDto.LocationType,
            BuildingId = apiDto.BuildingId,
            BuildingName = apiDto.BuildingName,
            FloorId = apiDto.FloorId,
            FloorName = apiDto.FloorName,
            ParentLocationId = apiDto.ParentLocationId,
            HierarchyPath = apiDto.HierarchyPath,
            Area = apiDto.Area,
            Capacity = apiDto.Capacity,
            Status = apiDto.Status
        };
    }
}
