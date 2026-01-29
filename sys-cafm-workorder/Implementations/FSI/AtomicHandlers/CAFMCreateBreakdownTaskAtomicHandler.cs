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
/// Atomic handler for CAFM CreateBreakdownTask SOAP operation.
/// Creates a new breakdown task in CAFM.
/// </summary>
public class CAFMCreateBreakdownTaskAtomicHandler : IAtomicHandler<CAFMCreateBreakdownTaskDownstreamResponseDto>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<CAFMCreateBreakdownTaskAtomicHandler> _logger;
    private const string StepName = nameof(CAFMCreateBreakdownTaskAtomicHandler);

    public CAFMCreateBreakdownTaskAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<CAFMCreateBreakdownTaskAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CAFMCreateBreakdownTaskDownstreamResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (CAFMCreateBreakdownTaskDownstreamRequestDto)downStreamRequestDTO;
        
        _logger.Info($"Executing CAFM CreateBreakdownTask to {request.Url} for CallerSourceId: {request.CallerSourceId}");

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
            _logger.Error($"CAFM CreateBreakdownTask failed with status {response.StatusCode}: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: response.StatusCode,
                error: ("CAFM_CREATE_TASK_FAILED", $"CAFM CreateBreakdownTask failed with status {response.StatusCode}"),
                stepName: StepName);
        }

        // Parse response
        var parsedResponse = CAFMCreateBreakdownTaskDownstreamResponseDto.FromSoapXml(responseBody);

        if (!parsedResponse.IsSuccess)
        {
            _logger.Error($"CAFM CreateBreakdownTask succeeded HTTP but no TaskId returned: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: HttpStatusCode.BadGateway,
                error: ("CAFM_CREATE_TASK_NO_ID", "CAFM CreateBreakdownTask succeeded but no task ID was returned"),
                stepName: StepName);
        }

        _logger.Info($"CAFM CreateBreakdownTask successful. TaskId: {parsedResponse.TaskId}");
        return parsedResponse;
    }
}
