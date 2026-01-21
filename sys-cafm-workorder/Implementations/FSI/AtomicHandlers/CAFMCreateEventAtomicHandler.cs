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
/// Atomic handler for CAFM CreateEvent SOAP operation.
/// Creates a new event (links a task) in CAFM.
/// </summary>
public class CAFMCreateEventAtomicHandler : IAtomicHandler<CAFMCreateEventDownstreamResponseDto>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<CAFMCreateEventAtomicHandler> _logger;
    private const string StepName = nameof(CAFMCreateEventAtomicHandler);

    public CAFMCreateEventAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<CAFMCreateEventAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CAFMCreateEventDownstreamResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (CAFMCreateEventDownstreamRequestDto)downStreamRequestDTO;
        
        _logger.Info($"Executing CAFM CreateEvent to {request.Url} for TaskId: {request.TaskId}");

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
            _logger.Error($"CAFM CreateEvent failed with status {response.StatusCode}: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: response.StatusCode,
                error: ("CAFM_CREATE_EVENT_FAILED", $"CAFM CreateEvent failed with status {response.StatusCode}"),
                stepName: StepName);
        }

        // Parse response
        var parsedResponse = CAFMCreateEventDownstreamResponseDto.FromSoapXml(responseBody);

        if (!parsedResponse.IsSuccess)
        {
            _logger.Error($"CAFM CreateEvent failed: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: HttpStatusCode.BadGateway,
                error: ("CAFM_CREATE_EVENT_ERROR", "CAFM CreateEvent failed"),
                stepName: StepName);
        }

        _logger.Info($"CAFM CreateEvent successful for TaskId: {request.TaskId}");
        return parsedResponse;
    }
}
