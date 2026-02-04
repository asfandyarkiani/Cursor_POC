using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.DTOs.Downstream.CAFM;
using System.Text;

namespace sys_cafm_workorder.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic handler for CAFM GetLocationsByDto SOAP operation.
/// Retrieves locations from CAFM based on filter criteria.
/// </summary>
public class CAFMGetLocationsAtomicHandler : IAtomicHandler<CAFMGetLocationsDownstreamResponseDto>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<CAFMGetLocationsAtomicHandler> _logger;
    private const string StepName = nameof(CAFMGetLocationsAtomicHandler);

    public CAFMGetLocationsAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<CAFMGetLocationsAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CAFMGetLocationsDownstreamResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (CAFMGetLocationsDownstreamRequestDto)downStreamRequestDTO;
        
        _logger.Info($"Executing CAFM GetLocationsByDto to {request.Url} for PropertyName: {request.PropertyName}");

        // Validate request
        request.ValidateDownStreamRequestParameters();

        // Build SOAP request
        var soapXml = request.ToSoapXml();

        // Prepare headers
        var requestHeaders = new List<Tuple<string, string>>
        {
            new("Content-Type", "text/xml; charset=utf-8"),
            new("SOAPAction", request.SoapAction)
        };

        // Make HTTP request
        var response = await _httpClient.SendAsync(
            HttpMethod.Post,
            request.Url,
            () => new StringContent(soapXml, Encoding.UTF8, "text/xml"),
            requestHeaders);

        // Read response
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error($"CAFM GetLocationsByDto failed with status {response.StatusCode}: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: response.StatusCode,
                error: ("CAFM_GET_LOCATIONS_FAILED", $"CAFM GetLocationsByDto failed with status {response.StatusCode}"),
                stepName: StepName);
        }

        // Parse response
        var parsedResponse = CAFMGetLocationsDownstreamResponseDto.FromSoapXml(responseBody);

        _logger.Info($"CAFM GetLocationsByDto successful. Found {parsedResponse.Locations.Count} locations.");
        return parsedResponse;
    }
}
