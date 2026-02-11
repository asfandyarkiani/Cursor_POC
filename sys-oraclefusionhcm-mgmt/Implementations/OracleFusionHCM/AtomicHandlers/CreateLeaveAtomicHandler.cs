using AlGhurair.Core.SystemLayer.Handlers;
using AlGhurair.Core.SystemLayer.Middlewares;
using AlGhurair.SystemLayer.OracleFusionHCM.ConfigModels;
using AlGhurair.SystemLayer.OracleFusionHCM.Constants;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.AtomicHandlerDTOs;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.DownstreamDTOs;
using AlGhurair.SystemLayer.OracleFusionHCM.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlGhurair.SystemLayer.OracleFusionHCM.Implementations.OracleFusionHCM.AtomicHandlers;

/// <summary>
/// Atomic Handler for creating leave in Oracle Fusion HCM
/// Makes single HTTP POST call to Oracle Fusion HCM absences endpoint
/// </summary>
public class CreateLeaveAtomicHandler : IAtomicHandler<CreateLeaveHandlerReqDTO, CreateLeaveApiResDTO>
{
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _appConfigs;
    private readonly ILogger<CreateLeaveAtomicHandler> _logger;

    public CreateLeaveAtomicHandler(
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> appConfigs,
        ILogger<CreateLeaveAtomicHandler> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes HTTP POST to Oracle Fusion HCM to create leave
    /// </summary>
    public async Task<CreateLeaveApiResDTO> ExecuteAsync(CreateLeaveHandlerReqDTO request)
    {
        _logger.LogInformation(InfoConstants.CALLING_ORACLE_FUSION_HCM_API);

        // Validate configuration
        if (string.IsNullOrWhiteSpace(_appConfigs.OracleFusionHCM.BaseUrl))
        {
            throw new InvalidOperationException($"{ErrorConstants.OFH_LVEMGT_0005}: Oracle Fusion HCM Base URL is not configured");
        }

        if (string.IsNullOrWhiteSpace(_appConfigs.OracleFusionHCM.AbsencesResourcePath))
        {
            throw new InvalidOperationException($"{ErrorConstants.OFH_LVEMGT_0005}: Oracle Fusion HCM Absences Resource Path is not configured");
        }

        if (string.IsNullOrWhiteSpace(_appConfigs.OracleFusionHCM.Username) || 
            string.IsNullOrWhiteSpace(_appConfigs.OracleFusionHCM.Password))
        {
            throw new InvalidOperationException($"{ErrorConstants.OFH_LVEMGT_0006}: Oracle Fusion HCM credentials are not configured");
        }

        // Build full URL
        string baseUrl = _appConfigs.OracleFusionHCM.BaseUrl.TrimEnd('/');
        string resourcePath = _appConfigs.OracleFusionHCM.AbsencesResourcePath.TrimStart('/');
        string fullUrl = $"{baseUrl}/{resourcePath}";

        _logger.LogInformation($"Calling Oracle Fusion HCM API: {fullUrl}");

        // Serialize request to JSON
        string jsonPayload = RestApiHelper.SerializeToJson(request);
        _logger.LogDebug($"Request payload: {jsonPayload}");

        // Create HTTP request
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, fullUrl)
        {
            Content = RestApiHelper.CreateJsonContent(jsonPayload)
        };

        // Add Basic Authentication header
        string authHeader = RestApiHelper.CreateBasicAuthHeader(
            _appConfigs.OracleFusionHCM.Username,
            _appConfigs.OracleFusionHCM.Password);
        httpRequestMessage.Headers.Add("Authorization", authHeader);

        // Add Accept header
        httpRequestMessage.Headers.Add("Accept", "application/json");

        // Execute HTTP request
        HttpResponseSnapshot httpResponseSnapshot = await _httpClient.SendAsync(httpRequestMessage);

        // Check HTTP status code
        if (!httpResponseSnapshot.IsSuccessStatusCode)
        {
            _logger.LogError($"{InfoConstants.ORACLE_FUSION_HCM_API_FAILED}: HTTP {httpResponseSnapshot.StatusCode}");
            _logger.LogError($"Response: {httpResponseSnapshot.Content}");

            throw new HttpRequestException(
                $"{ErrorConstants.OFH_LVEMGT_0003}: Oracle Fusion HCM API returned error. " +
                $"Status: {httpResponseSnapshot.StatusCode}, Response: {httpResponseSnapshot.Content}");
        }

        _logger.LogInformation(InfoConstants.ORACLE_FUSION_HCM_API_SUCCESS);

        // Deserialize response
        if (string.IsNullOrWhiteSpace(httpResponseSnapshot.Content))
        {
            throw new InvalidOperationException($"{ErrorConstants.OFH_LVEMGT_0004}: Oracle Fusion HCM API returned empty response");
        }

        CreateLeaveApiResDTO apiResponse;
        try
        {
            apiResponse = RestApiHelper.DeserializeJsonResponse<CreateLeaveApiResDTO>(httpResponseSnapshot.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{ErrorConstants.OFH_LVEMGT_0004}: Failed to deserialize Oracle Fusion HCM response");
            throw new InvalidOperationException(
                $"{ErrorConstants.OFH_LVEMGT_0004}: Failed to deserialize Oracle Fusion HCM response", ex);
        }

        // Validate response contains PersonAbsenceEntryId
        if (apiResponse.PersonAbsenceEntryId <= 0)
        {
            throw new InvalidOperationException(
                $"{ErrorConstants.OFH_LVEMGT_0010}: Oracle Fusion HCM response missing PersonAbsenceEntryId");
        }

        _logger.LogInformation($"Leave created successfully. PersonAbsenceEntryId: {apiResponse.PersonAbsenceEntryId}");

        return apiResponse;
    }
}
