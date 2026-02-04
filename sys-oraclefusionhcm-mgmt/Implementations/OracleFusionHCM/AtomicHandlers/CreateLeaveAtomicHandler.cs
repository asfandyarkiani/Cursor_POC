using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sys_oraclefusionhcm_mgmt.ConfigModels;
using sys_oraclefusionhcm_mgmt.Constants;
using sys_oraclefusionhcm_mgmt.DTO.AtomicHandlerDTOs;
using sys_oraclefusionhcm_mgmt.DTO.DownstreamDTOs;
using sys_oraclefusionhcm_mgmt.Helpers;
using System.Net.Http.Headers;
using System.Text;

namespace sys_oraclefusionhcm_mgmt.Implementations.OracleFusionHCM.AtomicHandlers;

/// <summary>
/// Atomic Handler for creating leave absence in Oracle Fusion HCM
/// Makes single HTTP POST call to Oracle Fusion HCM absences API
/// </summary>
public class CreateLeaveAtomicHandler : IAtomicHandler
{
    private readonly ILogger<CreateLeaveAtomicHandler> _logger;
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _appConfigs;
    
    public CreateLeaveAtomicHandler(
        ILogger<CreateLeaveAtomicHandler> logger,
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
    }
    
    /// <summary>
    /// Creates leave absence in Oracle Fusion HCM
    /// </summary>
    public async Task<HttpResponseSnapshot> CreateLeaveAsync(CreateLeaveHandlerReqDTO requestDto)
    {
        _logger.LogInformation(InfoConstants.CREATE_LEAVE_CALLING_DOWNSTREAM);
        
        // Validate configuration
        ValidateConfiguration();
        
        // Build full URL
        string fullUrl = BuildFullUrl();
        _logger.LogDebug("Oracle Fusion HCM API URL: {Url}", fullUrl);
        
        // Serialize request payload
        StringContent jsonContent = RestApiHelper.CreateJsonContent(requestDto);
        _logger.LogDebug("Request payload: {Payload}", RestApiHelper.SerializeToJson(requestDto));
        
        // Create HTTP request message
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, fullUrl)
        {
            Content = jsonContent
        };
        
        // Add Basic Authentication header
        string basicAuthCredentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_appConfigs.OracleFusionHCM.Username}:{_appConfigs.OracleFusionHCM.Password}")
        );
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuthCredentials);
        
        // Add standard headers
        httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        try
        {
            // Make HTTP call using CustomHTTPClient
            HttpResponseSnapshot responseSnapshot = await _httpClient.SendAsync(httpRequestMessage);
            
            _logger.LogInformation(InfoConstants.CREATE_LEAVE_RESPONSE_RECEIVED);
            _logger.LogDebug("Response status code: {StatusCode}", responseSnapshot.StatusCode);
            _logger.LogDebug("Response content: {Content}", responseSnapshot.Content);
            
            return responseSnapshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorConstants.OFH_CRLEAV_1001_MSG);
            throw;
        }
    }
    
    /// <summary>
    /// Validates Oracle Fusion HCM configuration
    /// </summary>
    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_appConfigs.OracleFusionHCM.BaseUrl))
        {
            _logger.LogError("Oracle Fusion HCM BaseUrl is not configured");
            throw new InvalidOperationException(ErrorConstants.OFH_SYSTEM_9002_MSG);
        }
        
        if (string.IsNullOrWhiteSpace(_appConfigs.OracleFusionHCM.ResourcePath))
        {
            _logger.LogError("Oracle Fusion HCM ResourcePath is not configured");
            throw new InvalidOperationException(ErrorConstants.OFH_SYSTEM_9002_MSG);
        }
        
        if (string.IsNullOrWhiteSpace(_appConfigs.OracleFusionHCM.Username) || 
            string.IsNullOrWhiteSpace(_appConfigs.OracleFusionHCM.Password))
        {
            _logger.LogError("Oracle Fusion HCM credentials are not configured");
            throw new InvalidOperationException(ErrorConstants.OFH_SYSTEM_9003_MSG);
        }
    }
    
    /// <summary>
    /// Builds full URL for Oracle Fusion HCM API call
    /// </summary>
    private string BuildFullUrl()
    {
        string baseUrl = _appConfigs.OracleFusionHCM.BaseUrl.TrimEnd('/');
        string resourcePath = _appConfigs.OracleFusionHCM.ResourcePath.TrimStart('/');
        return $"{baseUrl}/{resourcePath}";
    }
}
