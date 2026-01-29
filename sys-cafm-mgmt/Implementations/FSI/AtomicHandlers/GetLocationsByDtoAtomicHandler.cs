using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using AGI.SystemLayer.CAFM.ConfigModels;
using AGI.SystemLayer.CAFM.DTOs.Downstream;
using Core.Middlewares;
using Core.SystemLayer.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic Handler for GetLocationsByDto SOAP operation.
/// Retrieves location information from CAFM based on barcode.
/// </summary>
public class GetLocationsByDtoAtomicHandler : IAtomicHandler
{
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _config;
    private readonly ILogger<GetLocationsByDtoAtomicHandler> _logger;

    public GetLocationsByDtoAtomicHandler(
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> config,
        ILogger<GetLocationsByDtoAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<GetLocationsByDtoResponseDTO> GetLocationsAsync(
        GetLocationsByDtoRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var soapRequest = BuildGetLocationsSoapRequest(request.SessionId, request.BarCode);
            var url = $"{_config.CAFMSettings.BaseUrl}{_config.CAFMSettings.GetLocationsResourcePath}";

            _logger.LogInformation("CAFM GetLocations: Sending request to {Url} for BarCode: {BarCode}", url, request.BarCode);

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml"),
                new Dictionary<string, string>
                {
                    { "SOAPAction", _config.CAFMSettings.GetLocationsSoapAction }
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var locations = ParseLocationsFromResponse(responseContent);

            _logger.LogInformation("CAFM GetLocations: Retrieved {Count} locations", locations.Count);

            return new GetLocationsByDtoResponseDTO
            {
                Locations = locations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM GetLocations: Exception during GetLocationsByDto");
            throw;
        }
    }

    private string BuildGetLocationsSoapRequest(string sessionId, string barCode)
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi=""http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel""
                  xmlns:fsi1=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:GetLocationsByDto>
            <ns:sessionId>{System.Security.SecurityElement.Escape(sessionId)}</ns:sessionId>
            <ns:locationDto>
                <fsi1:BarCode>{System.Security.SecurityElement.Escape(barCode)}</fsi1:BarCode>
            </ns:locationDto>
        </ns:GetLocationsByDto>
    </soapenv:Body>
</soapenv:Envelope>";
    }

    private List<LocationData> ParseLocationsFromResponse(string soapResponse)
    {
        var locations = new List<LocationData>();

        try
        {
            var doc = XDocument.Parse(soapResponse);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";
            XNamespace fsi1 = "http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel";

            var locationElements = doc.Descendants(fsi1 + "LocationDto");

            foreach (var element in locationElements)
            {
                locations.Add(new LocationData
                {
                    LocationId = element.Element(fsi1 + "LocationId")?.Value,
                    LocationCode = element.Element(fsi1 + "LocationCode")?.Value,
                    LocationName = element.Element(fsi1 + "LocationName")?.Value,
                    BarCode = element.Element(fsi1 + "BarCode")?.Value,
                    BuildingId = element.Element(fsi1 + "BuildingId")?.Value,
                    FloorId = element.Element(fsi1 + "FloorId")?.Value,
                    RoomId = element.Element(fsi1 + "RoomId")?.Value
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM GetLocations: Error parsing locations from SOAP response");
        }

        return locations;
    }
}
