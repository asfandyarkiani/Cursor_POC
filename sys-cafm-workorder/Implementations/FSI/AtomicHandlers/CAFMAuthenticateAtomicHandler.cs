using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.DTOs.Downstream.CAFM;
using System.Net;
using System.Text;

namespace sys_cafm_workorder.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic handler for CAFM Authenticate SOAP operation.
/// Makes the actual HTTP call to the CAFM service.
/// </summary>
public class CAFMAuthenticateAtomicHandler : IAtomicHandler<CAFMAuthenticateDownstreamResponseDto>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<CAFMAuthenticateAtomicHandler> _logger;
    private const string StepName = nameof(CAFMAuthenticateAtomicHandler);

    public CAFMAuthenticateAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<CAFMAuthenticateAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CAFMAuthenticateDownstreamResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (CAFMAuthenticateDownstreamRequestDto)downStreamRequestDTO;
        
        _logger.Info($"Executing CAFM Authenticate to {request.Url}");

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
            _logger.Error($"CAFM Authenticate failed with status {response.StatusCode}: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: response.StatusCode,
                error: ("CAFM_AUTH_FAILED", $"CAFM authentication failed with status {response.StatusCode}"),
                stepName: StepName);
        }

        // Parse response
        var parsedResponse = CAFMAuthenticateDownstreamResponseDto.FromSoapXml(responseBody);

        if (!parsedResponse.IsSuccess)
        {
            _logger.Error($"CAFM Authenticate succeeded HTTP but no SessionId returned: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: HttpStatusCode.BadGateway,
                error: ("CAFM_AUTH_NO_SESSION", "CAFM authentication succeeded but no session ID was returned"),
                stepName: StepName);
        }

        _logger.Info($"CAFM Authenticate successful. SessionId received.");
        return parsedResponse;
    }
}
