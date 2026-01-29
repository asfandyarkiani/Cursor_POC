using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.DTOs.Downstream.CAFM;
using System.Text;

namespace sys_cafm_workorder.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic handler for CAFM GetInstructionSetsByDto SOAP operation.
/// Retrieves instruction sets from CAFM based on filter criteria.
/// </summary>
public class CAFMGetInstructionSetsAtomicHandler : IAtomicHandler<CAFMGetInstructionSetsDownstreamResponseDto>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<CAFMGetInstructionSetsAtomicHandler> _logger;
    private const string StepName = nameof(CAFMGetInstructionSetsAtomicHandler);

    public CAFMGetInstructionSetsAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<CAFMGetInstructionSetsAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CAFMGetInstructionSetsDownstreamResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (CAFMGetInstructionSetsDownstreamRequestDto)downStreamRequestDTO;
        
        _logger.Info($"Executing CAFM GetInstructionSetsByDto to {request.Url} for CategoryName: {request.CategoryName}");

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
            _logger.Error($"CAFM GetInstructionSetsByDto failed with status {response.StatusCode}: {responseBody}");
            throw new DownStreamApiFailureException(
                statusCode: response.StatusCode,
                error: ("CAFM_GET_INSTRUCTIONS_FAILED", $"CAFM GetInstructionSetsByDto failed with status {response.StatusCode}"),
                stepName: StepName);
        }

        // Parse response
        var parsedResponse = CAFMGetInstructionSetsDownstreamResponseDto.FromSoapXml(responseBody);

        _logger.Info($"CAFM GetInstructionSetsByDto successful. Found {parsedResponse.InstructionSets.Count} instruction sets.");
        return parsedResponse;
    }
}
