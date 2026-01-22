using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.Downstream;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SysCafmMgmt.Implementations.Fsi.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for FSI GetLocationsByDto SOAP operation
    /// </summary>
    public class GetLocationsByDtoAtomicHandler : IAtomicHandler<GetLocationsByDtoResponseDto>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<GetLocationsByDtoAtomicHandler> _logger;
        private readonly AppConfigs _config;

        private const string StepName = "FSI.GetLocationsByDto";

        public GetLocationsByDtoAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<GetLocationsByDtoAtomicHandler> logger,
            IOptions<AppConfigs> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<GetLocationsByDtoResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            var request = downStreamRequestDTO as GetLocationsByDtoRequestDto
                ?? throw new ArgumentException($"Expected {nameof(GetLocationsByDtoRequestDto)}", nameof(downStreamRequestDTO));

            request.ValidateDownStreamRequestParameters();

            _logger.Info($"Getting locations from CAFM FSI for code: {request.LocationCode}");

            var url = $"{_config.Cafm.BaseUrl}{_config.Cafm.BreakdownTaskResourcePath}";
            var soapEnvelope = request.ToSoapEnvelope();

            var headers = new List<Tuple<string, string>>
            {
                new("SOAPAction", _config.Cafm.SoapActionGetLocations),
                new("Content-Type", "text/xml; charset=utf-8")
            };

            var response = await _httpClient.SendAsync(
                HttpMethod.Post,
                url,
                () => new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"),
                headers
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.Error($"FSI GetLocationsByDto failed with status {response.StatusCode}: {errorContent}");

                throw new Core.SystemLayer.Exceptions.DownStreamApiFailureException(
                    message: "CAFM FSI GetLocationsByDto failed",
                    errorCode: "CAFM_GET_LOCATIONS_FAILED",
                    statusCode: response.StatusCode,
                    errorDetails: new List<string> { errorContent },
                    stepName: StepName
                );
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = ParseGetLocationsResponse(responseContent);

            _logger.Info($"Retrieved {result.Locations?.Count ?? 0} locations from CAFM FSI");
            return result;
        }

        private GetLocationsByDtoResponseDto ParseGetLocationsResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                XNamespace ns1 = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns2 = "http://www.fsi.co.uk/services/evolution/04/09";
                XNamespace ns3 = "http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel";

                var body = doc.Descendants(ns1 + "Body").FirstOrDefault();
                var getLocationsResponse = body?.Descendants(ns2 + "GetLocationsByDtoResponse").FirstOrDefault();
                var getLocationsResult = getLocationsResponse?.Descendants(ns2 + "GetLocationsByDtoResult").FirstOrDefault();
                var locations = getLocationsResult?.Descendants(ns3 + "LocationDto");

                var result = new GetLocationsByDtoResponseDto
                {
                    Locations = locations?.Select(loc => new LocationDto
                    {
                        LocationId = loc.Descendants(ns3 + "Id").FirstOrDefault()?.Value,
                        Code = loc.Descendants(ns3 + "Code").FirstOrDefault()?.Value,
                        Description = loc.Descendants(ns3 + "Description").FirstOrDefault()?.Value,
                        BuildingId = loc.Descendants(ns3 + "BuildingId").FirstOrDefault()?.Value,
                        BuildingName = loc.Descendants(ns3 + "BuildingName").FirstOrDefault()?.Value
                    }).ToList() ?? new List<LocationDto>()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to parse GetLocationsByDto response: {ex.Message}");
                return new GetLocationsByDtoResponseDto { Locations = new List<LocationDto>() };
            }
        }
    }
}
