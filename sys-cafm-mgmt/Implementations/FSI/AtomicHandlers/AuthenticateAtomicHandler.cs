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
/// Atomic handler for FSI Authenticate SOAP operation
/// Single responsibility: Login and get session ID
/// </summary>
public class AuthenticateAtomicHandler : IAtomicHandler<FsiAuthenticateResponseDTO>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly ILogger<AuthenticateAtomicHandler> _logger;
    private readonly FsiConfig _fsiConfig;
    private const string StepName = "FSI_Authenticate";

    public AuthenticateAtomicHandler(
        CustomHTTPClient httpClient,
        ILogger<AuthenticateAtomicHandler> logger,
        IOptions<AppConfigs> appConfigs)
    {
        _httpClient = httpClient;
        _logger = logger;
        _fsiConfig = appConfigs.Value.Fsi;
    }

    public async Task<FsiAuthenticateResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
    {
        var request = (FsiAuthenticateRequestDTO)downStreamRequestDTO;
        request.ValidateDownStreamRequestParameters();

        var url = $"{_fsiConfig.BaseUrl}{_fsiConfig.LoginResourcePath}";
        var soapBody = request.ToSoapRequest();

        _logger.Info($"Calling FSI Authenticate at {url}");

        var headers = new List<Tuple<string, string>>
        {
            Tuple.Create("SOAPAction", _fsiConfig.SoapActionLogin),
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
            _logger.Error($"FSI Authenticate failed with status {response.StatusCode}");
            throw new DownStreamApiFailureException(
                $"FSI Authentication failed: {response.StatusCode}",
                "FSI_AUTH_FAILED",
                response.StatusCode,
                new List<string> { responseBody },
                StepName);
        }

        // Parse SOAP response for SessionId
        var result = ParseAuthenticateResponse(responseBody);
        
        if (string.IsNullOrWhiteSpace(result.SessionId))
        {
            _logger.Error("FSI Authenticate returned empty SessionId");
            throw new DownStreamApiFailureException(
                "CAFM Log In Failed. CAFM Log In API Responded with Blank Response",
                "FSI_AUTH_EMPTY_SESSION",
                HttpStatusCode.Unauthorized,
                new List<string> { "SessionId is empty in response" },
                StepName);
        }

        _logger.Info($"FSI Authenticate successful, SessionId obtained");
        return result;
    }

    private FsiAuthenticateResponseDTO ParseAuthenticateResponse(string soapResponse)
    {
        var result = new FsiAuthenticateResponseDTO();

        // Extract SessionId using regex (simpler than full XML parsing for SOAP)
        var sessionIdMatch = Regex.Match(soapResponse, @"<[^:]*:?SessionId[^>]*>([^<]+)</[^:]*:?SessionId>");
        if (sessionIdMatch.Success)
        {
            result.SessionId = sessionIdMatch.Groups[1].Value;
        }

        var operationResultMatch = Regex.Match(soapResponse, @"<[^:]*:?OperationResult[^>]*>([^<]+)</[^:]*:?OperationResult>");
        if (operationResultMatch.Success)
        {
            result.OperationResult = operationResultMatch.Groups[1].Value;
        }

        var versionMatch = Regex.Match(soapResponse, @"<[^:]*:?EvolutionVersion[^>]*>([^<]+)</[^:]*:?EvolutionVersion>");
        if (versionMatch.Success)
        {
            result.EvolutionVersion = versionMatch.Groups[1].Value;
        }

        return result;
    }
}
