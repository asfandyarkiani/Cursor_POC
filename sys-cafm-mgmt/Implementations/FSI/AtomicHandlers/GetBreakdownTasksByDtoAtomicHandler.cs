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
/// Atomic handler for FSI GetBreakdownTasksByDto SOAP operation
/// Single responsibility: Get breakdown task details by task number
/// </summary>
public class GetBreakdownTasksByDtoAtomicHandler : IAtomicHandler<FsiGetBreakdownTasksByDtoResponseDTO>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<GetBreakdownTasksByDtoAtomicHandler> _logger;
    private readonly FsiConfig _fsiConfig;
    private const string StepName = "FSI_GetBreakdownTasksByDto";

    public GetBreakdownTasksByDtoAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<GetBreakdownTasksByDtoAtomicHandler> logger,
        IOptions<AppConfigs> appConfigs)
    {
        _httpClient = httpClient;
        _logger = logger;
        _fsiConfig = appConfigs.Value.Fsi;
    }

    public async Task<FsiGetBreakdownTasksByDtoResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (FsiGetBreakdownTasksByDtoRequestDTO)downStreamRequestDTO;
        request.ValidateDownStreamRequestParameters();

        var url = $"{_fsiConfig.BaseUrl}{_fsiConfig.BreakdownTaskResourcePath}";
        var soapBody = request.ToSoapRequest();

        _logger.Info($"Calling FSI GetBreakdownTasksByDto at {url} for TaskNumber: {request.TaskNumber}");

        var headers = new List<Tuple<string, string>>
        {
            Tuple.Create("SOAPAction", _fsiConfig.SoapActionGetBreakdownTasks),
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
            _logger.Error($"FSI GetBreakdownTasksByDto failed with status {response.StatusCode}");
            throw new DownStreamApiFailureException(
                $"FSI GetBreakdownTasksByDto failed: {response.StatusCode}",
                "FSI_GET_TASKS_FAILED",
                response.StatusCode,
                new List<string> { responseBody },
                StepName);
        }

        var result = ParseGetBreakdownTasksResponse(responseBody);

        _logger.Info($"FSI GetBreakdownTasksByDto successful - TaskId: {result.TaskId}");
        return result;
    }

    private FsiGetBreakdownTasksByDtoResponseDTO ParseGetBreakdownTasksResponse(string soapResponse)
    {
        var result = new FsiGetBreakdownTasksByDtoResponseDTO();

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

        // Extract Status
        var statusMatch = Regex.Match(soapResponse, @"<[^:]*:?BDET_STATUS[^>]*>([^<]+)</[^:]*:?BDET_STATUS>");
        if (statusMatch.Success)
        {
            result.Status = statusMatch.Groups[1].Value;
        }

        // Extract BuildingId
        var buildingIdMatch = Regex.Match(soapResponse, @"<[^:]*:?BDET_FKEY_BLO_SEQ[^>]*>([^<]+)</[^:]*:?BDET_FKEY_BLO_SEQ>");
        if (buildingIdMatch.Success)
        {
            result.BuildingId = buildingIdMatch.Groups[1].Value;
        }

        // Extract LocationId
        var locationIdMatch = Regex.Match(soapResponse, @"<[^:]*:?BDET_FKEY_LOC_SEQ[^>]*>([^<]+)</[^:]*:?BDET_FKEY_LOC_SEQ>");
        if (locationIdMatch.Success)
        {
            result.LocationId = locationIdMatch.Groups[1].Value;
        }

        return result;
    }
}
