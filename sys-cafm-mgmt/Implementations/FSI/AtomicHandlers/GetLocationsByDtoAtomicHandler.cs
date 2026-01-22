using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.Downstream;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SysCafmMgmt.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic handler for FSI GetLocationsByDto SOAP operation
/// Single responsibility: Get location details by barcode
/// </summary>
public class GetLocationsByDtoAtomicHandler : IAtomicHandler<FsiGetLocationsByDtoResponseDTO>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<GetLocationsByDtoAtomicHandler> _logger;
    private readonly FsiConfig _fsiConfig;
    private const string StepName = "FSI_GetLocationsByDto";

    public GetLocationsByDtoAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<GetLocationsByDtoAtomicHandler> logger,
        IOptions<AppConfigs> appConfigs)
    {
        _httpClient = httpClient;
        _logger = logger;
        _fsiConfig = appConfigs.Value.Fsi;
    }

    public async Task<FsiGetLocationsByDtoResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (FsiGetLocationsByDtoRequestDTO)downStreamRequestDTO;
        request.ValidateDownStreamRequestParameters();

        var url = $"{_fsiConfig.BaseUrl}{_fsiConfig.GetLocationsResourcePath}";
        var soapBody = request.ToSoapRequest();

        _logger.Info($"Calling FSI GetLocationsByDto at {url} for BarCode: {request.BarCode}");

        var headers = new List<Tuple<string, string>>
        {
            Tuple.Create("SOAPAction", _fsiConfig.SoapActionGetLocations),
            Tuple.Create("Content-Type", "text/xml; charset=utf-8")
        };

        var response = await _httpClient.SendAsync(
            HttpMethod.Post,
            url,
            () => new StringContent(soapBody, Encoding.UTF8, "text/xml"),
            headers);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error($"FSI GetLocationsByDto failed with status {response.StatusCode}");
            throw new DownStreamApiFailureException(
                $"FSI GetLocationsByDto failed: {response.StatusCode}",
                "FSI_GET_LOCATIONS_FAILED",
                response.StatusCode,
                new List<string> { responseBody },
                StepName);
        }

        var result = ParseGetLocationsResponse(responseBody);

        if (string.IsNullOrWhiteSpace(result.BuildingId) || string.IsNullOrWhiteSpace(result.LocationId))
        {
            _logger.Warn($"FSI GetLocationsByDto returned incomplete location data for BarCode: {request.BarCode}");
            throw new DownStreamApiFailureException(
                $"Location not found for BarCode: {request.BarCode}",
                "FSI_LOCATION_NOT_FOUND",
                HttpStatusCode.NotFound,
                new List<string> { $"BuildingId: {result.BuildingId}, LocationId: {result.LocationId}" },
                StepName);
        }

        _logger.Info($"FSI GetLocationsByDto successful - BuildingId: {result.BuildingId}, LocationId: {result.LocationId}");
        return result;
    }

    private FsiGetLocationsByDtoResponseDTO ParseGetLocationsResponse(string soapResponse)
    {
        var result = new FsiGetLocationsByDtoResponseDTO();

        // Extract BuildingId
        var buildingIdMatch = Regex.Match(soapResponse, @"<[^:]*:?BuildingId[^>]*>([^<]+)</[^:]*:?BuildingId>");
        if (buildingIdMatch.Success)
        {
            result.BuildingId = buildingIdMatch.Groups[1].Value;
        }

        // Extract LocationId
        var locationIdMatch = Regex.Match(soapResponse, @"<[^:]*:?LocationId[^>]*>([^<]+)</[^:]*:?LocationId>");
        if (locationIdMatch.Success)
        {
            result.LocationId = locationIdMatch.Groups[1].Value;
        }

        // Extract LocationName
        var locationNameMatch = Regex.Match(soapResponse, @"<[^:]*:?LocationName[^>]*>([^<]+)</[^:]*:?LocationName>");
        if (locationNameMatch.Success)
        {
            result.LocationName = locationNameMatch.Groups[1].Value;
        }

        // Extract BarCode
        var barCodeMatch = Regex.Match(soapResponse, @"<[^:]*:?BarCode[^>]*>([^<]+)</[^:]*:?BarCode>");
        if (barCodeMatch.Success)
        {
            result.BarCode = barCodeMatch.Groups[1].Value;
        }

        return result;
    }
}
