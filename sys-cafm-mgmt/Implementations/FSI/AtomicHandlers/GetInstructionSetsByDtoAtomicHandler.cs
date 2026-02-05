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
/// Atomic handler for FSI GetInstructionSetsByDto SOAP operation
/// Single responsibility: Get instruction set details by description/subcategory
/// </summary>
public class GetInstructionSetsByDtoAtomicHandler : IAtomicHandler<FsiGetInstructionSetsByDtoResponseDTO>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<GetInstructionSetsByDtoAtomicHandler> _logger;
    private readonly FsiConfig _fsiConfig;
    private const string StepName = "FSI_GetInstructionSetsByDto";

    public GetInstructionSetsByDtoAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<GetInstructionSetsByDtoAtomicHandler> logger,
        IOptions<AppConfigs> appConfigs)
    {
        _httpClient = httpClient;
        _logger = logger;
        _fsiConfig = appConfigs.Value.Fsi;
    }

    public async Task<FsiGetInstructionSetsByDtoResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (FsiGetInstructionSetsByDtoRequestDTO)downStreamRequestDTO;
        request.ValidateDownStreamRequestParameters();

        var url = $"{_fsiConfig.BaseUrl}{_fsiConfig.BreakdownTaskResourcePath}";
        var soapBody = request.ToSoapRequest();

        _logger.Info($"Calling FSI GetInstructionSetsByDto at {url} for SubCategory: {request.InDescription}");

        var headers = new List<Tuple<string, string>>
        {
            Tuple.Create("SOAPAction", _fsiConfig.SoapActionGetInstructionSets),
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
            _logger.Error($"FSI GetInstructionSetsByDto failed with status {response.StatusCode}");
            throw new DownStreamApiFailureException(
                $"FSI GetInstructionSetsByDto failed: {response.StatusCode}",
                "FSI_GET_INSTRUCTION_SETS_FAILED",
                response.StatusCode,
                new List<string> { responseBody },
                StepName);
        }

        var result = ParseGetInstructionSetsResponse(responseBody);

        if (string.IsNullOrWhiteSpace(result.InstructionId))
        {
            _logger.Warn($"FSI GetInstructionSetsByDto returned no instruction for SubCategory: {request.InDescription}");
            throw new DownStreamApiFailureException(
                $"Instruction set not found for SubCategory: {request.InDescription}",
                "FSI_INSTRUCTION_SET_NOT_FOUND",
                HttpStatusCode.NotFound,
                new List<string> { $"SubCategory: {request.InDescription}" },
                StepName);
        }

        _logger.Info($"FSI GetInstructionSetsByDto successful - InstructionId: {result.InstructionId}");
        return result;
    }

    private FsiGetInstructionSetsByDtoResponseDTO ParseGetInstructionSetsResponse(string soapResponse)
    {
        var result = new FsiGetInstructionSetsByDtoResponseDTO();

        // Extract IN_FKEY_CAT_SEQ (CategoryId)
        var categoryIdMatch = Regex.Match(soapResponse, @"<[^:]*:?IN_FKEY_CAT_SEQ[^>]*>([^<]+)</[^:]*:?IN_FKEY_CAT_SEQ>");
        if (categoryIdMatch.Success)
        {
            result.CategoryId = categoryIdMatch.Groups[1].Value;
        }

        // Extract IN_FKEY_LAB_SEQ (DisciplineId)
        var disciplineIdMatch = Regex.Match(soapResponse, @"<[^:]*:?IN_FKEY_LAB_SEQ[^>]*>([^<]+)</[^:]*:?IN_FKEY_LAB_SEQ>");
        if (disciplineIdMatch.Success)
        {
            result.DisciplineId = disciplineIdMatch.Groups[1].Value;
        }

        // Extract IN_FKEY_PRI_SEQ (PriorityId)
        var priorityIdMatch = Regex.Match(soapResponse, @"<[^:]*:?IN_FKEY_PRI_SEQ[^>]*>([^<]+)</[^:]*:?IN_FKEY_PRI_SEQ>");
        if (priorityIdMatch.Success)
        {
            result.PriorityId = priorityIdMatch.Groups[1].Value;
        }

        // Extract IN_SEQ (InstructionId)
        var instructionIdMatch = Regex.Match(soapResponse, @"<[^:]*:?IN_SEQ[^>]*>([^<]+)</[^:]*:?IN_SEQ>");
        if (instructionIdMatch.Success)
        {
            result.InstructionId = instructionIdMatch.Groups[1].Value;
        }

        // Extract IN_DESCRIPTION
        var descriptionMatch = Regex.Match(soapResponse, @"<[^:]*:?IN_DESCRIPTION[^>]*>([^<]+)</[^:]*:?IN_DESCRIPTION>");
        if (descriptionMatch.Success)
        {
            result.InstructionDescription = descriptionMatch.Groups[1].Value;
        }

        return result;
    }
}
