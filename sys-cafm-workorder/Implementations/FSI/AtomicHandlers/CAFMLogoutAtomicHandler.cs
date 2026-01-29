using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.DTOs.Downstream.CAFM;
using System.Text;

namespace sys_cafm_workorder.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic handler for CAFM Logout SOAP operation.
/// Makes the actual HTTP call to terminate the session.
/// </summary>
public class CAFMLogoutAtomicHandler : IAtomicHandler<bool>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<CAFMLogoutAtomicHandler> _logger;
    private const string StepName = nameof(CAFMLogoutAtomicHandler);

    public CAFMLogoutAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<CAFMLogoutAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (CAFMLogoutDownstreamRequestDto)downStreamRequestDTO;
        
        _logger.Info($"Executing CAFM Logout to {request.Url}");

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
            _logger.Error($"CAFM Logout failed with status {response.StatusCode}: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: response.StatusCode,
                error: ("CAFM_LOGOUT_FAILED", $"CAFM logout failed with status {response.StatusCode}"),
                stepName: StepName);
        }

        _logger.Info("CAFM Logout successful.");
        return true;
    }
}
