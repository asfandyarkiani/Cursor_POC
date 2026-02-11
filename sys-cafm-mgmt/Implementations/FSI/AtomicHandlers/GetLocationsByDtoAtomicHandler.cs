using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.DownStream;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Core.Exceptions;

namespace SysCafmMgmt.Implementations.FSI.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for GetLocationsByDto SOAP operation
    /// Retrieves locations from FSI CAFM system based on search criteria
    /// </summary>
    public class GetLocationsByDtoAtomicHandler : IAtomicHandler<GetLocationsByDtoSoapResponseDTO>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<GetLocationsByDtoAtomicHandler> _logger;
        private readonly AppConfigs _appConfigs;

        public GetLocationsByDtoAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<GetLocationsByDtoAtomicHandler> logger,
            IOptions<AppConfigs> appConfigs)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
        }

        public async Task<GetLocationsByDtoSoapResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            if (downStreamRequestDTO is not GetLocationsByDtoSoapRequestDTO request)
            {
                throw new ArgumentException("Invalid request DTO type", nameof(downStreamRequestDTO));
            }

            _logger.Info($"[GetLocationsByDtoAtomicHandler] Getting locations for property: {request.PropertyName}");

            try
            {
                // Build SOAP envelope
                string soapEnvelope = BuildGetLocationsSoapEnvelope(request);

                // Prepare HTTP request
                string url = $"{_appConfigs.CafmSettings.BaseUrl}{_appConfigs.CafmSettings.LoginResourcePath}";
                
                var headers = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("SOAPAction", _appConfigs.CafmSettings.GetLocationsByDtoSoapAction),
                    new Tuple<string, string>("Content-Type", "text/xml; charset=utf-8")
                };

                // Send SOAP request
                HttpResponseMessage response = await _httpClient.SendAsync(
                    HttpMethod.Post,
                    url,
                    () => new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"),
                    headers
                );

                // Parse response
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"[GetLocationsByDtoAtomicHandler] GetLocations failed with status: {response.StatusCode}");
                    throw new HTTPBaseException(
                        $"GetLocations failed: {response.StatusCode}",
                        "CAFM_GET_LOCATIONS_FAILED",
                        response.StatusCode,
                        new List<string> { responseBody },
                        "GetLocationsByDtoAtomicHandler",
                        true
                    );
                }

                // Parse SOAP response
                var result = ParseGetLocationsResponse(responseBody);

                _logger.Info($"[GetLocationsByDtoAtomicHandler] Retrieved {result.Locations?.Count ?? 0} locations");
                return result;
            }
            catch (HTTPBaseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetLocationsByDtoAtomicHandler] Exception: {ex.Message}");
                throw new HTTPBaseException(
                    $"GetLocations exception: {ex.Message}",
                    "CAFM_GET_LOCATIONS_EXCEPTION",
                    HttpStatusCode.InternalServerError,
                    new List<string> { ex.ToString() },
                    "GetLocationsByDtoAtomicHandler",
                    true
                );
            }
        }

        private string BuildGetLocationsSoapEnvelope(GetLocationsByDtoSoapRequestDTO request)
        {
            // TODO: Update SOAP envelope structure based on actual CAFM WSDL
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:GetLocationsByDto>
         <ns:sessionId>{System.Security.SecurityElement.Escape(request.SessionId ?? string.Empty)}</ns:sessionId>
         <ns:locationCode>{System.Security.SecurityElement.Escape(request.LocationCode ?? string.Empty)}</ns:locationCode>
         <ns:propertyName>{System.Security.SecurityElement.Escape(request.PropertyName ?? string.Empty)}</ns:propertyName>
      </ns:GetLocationsByDto>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        private GetLocationsByDtoSoapResponseDTO ParseGetLocationsResponse(string soapResponse)
        {
            try
            {
                // TODO: Update parsing logic based on actual CAFM SOAP response structure
                XDocument doc = XDocument.Parse(soapResponse);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                var locations = new List<LocationSoapDTO>();

                var locationElements = doc.Descendants(ns + "Location");
                foreach (var locationElement in locationElements)
                {
                    locations.Add(new LocationSoapDTO
                    {
                        LocationId = locationElement.Element(ns + "LocationId")?.Value,
                        LocationCode = locationElement.Element(ns + "LocationCode")?.Value,
                        LocationName = locationElement.Element(ns + "LocationName")?.Value,
                        PropertyName = locationElement.Element(ns + "PropertyName")?.Value,
                        BuildingName = locationElement.Element(ns + "BuildingName")?.Value,
                        FloorName = locationElement.Element(ns + "FloorName")?.Value
                    });
                }

                return new GetLocationsByDtoSoapResponseDTO
                {
                    Success = true,
                    Locations = locations,
                    RawResponse = soapResponse
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetLocationsByDtoAtomicHandler] Failed to parse response: {ex.Message}");
                return new GetLocationsByDtoSoapResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"Failed to parse response: {ex.Message}",
                    RawResponse = soapResponse
                };
            }
        }
    }
}
