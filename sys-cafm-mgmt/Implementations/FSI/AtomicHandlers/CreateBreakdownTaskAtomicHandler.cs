using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.Downstream;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SysCafmMgmt.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic handler for FSI CreateBreakdownTask SOAP operation
/// Single responsibility: Create a breakdown task (work order) in CAFM
/// </summary>
public class CreateBreakdownTaskAtomicHandler : IAtomicHandler<FsiCreateBreakdownTaskResponseDTO>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<CreateBreakdownTaskAtomicHandler> _logger;
    private readonly FsiConfig _fsiConfig;
    private const string StepName = "FSI_CreateBreakdownTask";

    public CreateBreakdownTaskAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<CreateBreakdownTaskAtomicHandler> logger,
        IOptions<AppConfigs> appConfigs)
    {
        _httpClient = httpClient;
        _logger = logger;
        _fsiConfig = appConfigs.Value.Fsi;
    }

    public async Task<FsiCreateBreakdownTaskResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (FsiCreateBreakdownTaskRequestDTO)downStreamRequestDTO;
        request.ValidateDownStreamRequestParameters();

        var url = $"{_fsiConfig.BaseUrl}{_fsiConfig.BreakdownTaskResourcePath}";
        var soapBody = request.ToSoapRequest();

        _logger.Info($"Calling FSI CreateBreakdownTask at {url} for CallId: {request.CallId}");

        var headers = new List<Tuple<string, string>>
        {
            Tuple.Create("SOAPAction", _fsiConfig.SoapActionCreateBreakdownTask),
            Tuple.Create("Content-Type", "text/xml; charset=utf-8")
        };

        var response = await _httpClient.SendAsync(
            HttpMethod.Post,
            url,
            () => new StringContent(soapBody, Encoding.UTF8, "text/xml"),
            headers);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error($"FSI CreateBreakdownTask failed with status {response.StatusCode}");
            throw new DownStreamApiFailureException(
                $"FSI CreateBreakdownTask failed: {response.StatusCode}",
                "FSI_CREATE_TASK_FAILED",
                response.StatusCode,
                new List<string> { responseBody },
                StepName);
        }

        var result = ParseCreateBreakdownTaskResponse(responseBody);

        if (string.IsNullOrWhiteSpace(result.TaskId) && string.IsNullOrWhiteSpace(result.TaskNumber))
        {
            _logger.Error($"FSI CreateBreakdownTask returned no task identifier");
            throw new DownStreamApiFailureException(
                "FSI CreateBreakdownTask returned empty response",
                "FSI_CREATE_TASK_EMPTY_RESPONSE",
                HttpStatusCode.InternalServerError,
                new List<string> { "TaskId and TaskNumber are both empty" },
                StepName);
        }

        _logger.Info($"FSI CreateBreakdownTask successful - TaskId: {result.TaskId}, TaskNumber: {result.TaskNumber}");
        return result;
    }

    private FsiCreateBreakdownTaskResponseDTO ParseCreateBreakdownTaskResponse(string soapResponse)
    {
        var result = new FsiCreateBreakdownTaskResponseDTO();

        // Extract BDET_SEQ (TaskId)
        var taskIdMatch = Regex.Match(soapResponse, @"<[^:]*:?BDET_SEQ[^>]*>([^<]+)</[^:]*:?BDET_SEQ>");
        if (taskIdMatch.Success)
        {
            result.TaskId = taskIdMatch.Groups[1].Value;
        }

        // Extract BDET_NO (TaskNumber)
        var taskNumberMatch = Regex.Match(soapResponse, @"<[^:]*:?BDET_NO[^>]*>([^<]+)</[^:]*:?BDET_NO>");
        if (taskNumberMatch.Success)
        {
            result.TaskNumber = taskNumberMatch.Groups[1].Value;
        }

        // Extract OperationResult
        var operationResultMatch = Regex.Match(soapResponse, @"<[^:]*:?OperationResult[^>]*>([^<]+)</[^:]*:?OperationResult>");
        if (operationResultMatch.Success)
        {
            result.OperationResult = operationResultMatch.Groups[1].Value;
        }

        // Check for error messages
        var errorMatch = Regex.Match(soapResponse, @"<[^:]*:?(?:Message|ErrorMessage|Error)[^>]*>([^<]+)</[^:]*:?(?:Message|ErrorMessage|Error)>");
        if (errorMatch.Success)
        {
            result.Message = errorMatch.Groups[1].Value;
        }

        return result;
    }
}
