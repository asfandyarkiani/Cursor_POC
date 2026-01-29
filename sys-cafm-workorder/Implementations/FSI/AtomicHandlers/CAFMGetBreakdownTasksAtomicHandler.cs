using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.DTOs.Downstream.CAFM;
using System.Text;

namespace sys_cafm_workorder.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic handler for CAFM GetBreakdownTasksByDto SOAP operation.
/// Retrieves breakdown tasks from CAFM based on filter criteria.
/// </summary>
public class CAFMGetBreakdownTasksAtomicHandler : IAtomicHandler<CAFMGetBreakdownTasksDownstreamResponseDto>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<CAFMGetBreakdownTasksAtomicHandler> _logger;
    private const string StepName = nameof(CAFMGetBreakdownTasksAtomicHandler);

    public CAFMGetBreakdownTasksAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<CAFMGetBreakdownTasksAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CAFMGetBreakdownTasksDownstreamResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (CAFMGetBreakdownTasksDownstreamRequestDto)downStreamRequestDTO;
        
        _logger.Info($"Executing CAFM GetBreakdownTasksByDto to {request.Url} for CallerSourceId: {request.CallerSourceId}");

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
            _logger.Error($"CAFM GetBreakdownTasksByDto failed with status {response.StatusCode}: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: response.StatusCode,
                error: ("CAFM_GET_TASKS_FAILED", $"CAFM GetBreakdownTasksByDto failed with status {response.StatusCode}"),
                stepName: StepName);
        }

        // Parse response
        var parsedResponse = CAFMGetBreakdownTasksDownstreamResponseDto.FromSoapXml(responseBody);

        _logger.Info($"CAFM GetBreakdownTasksByDto successful. Found {parsedResponse.Tasks.Count} tasks.");
        return parsedResponse;
    }
}
