using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Core.SystemLayer.Handlers;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.ConfigModels;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Constants;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.AtomicHandlerDTOs;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Helpers;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Implementations.BuddyAppMicroservice.AtomicHandlers;

/// <summary>
/// Atomic handler for sending push notifications to Buddy App microservice
/// Makes single HTTP POST call to downstream API
/// </summary>
public class SendPushNotificationAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
{
    private readonly ILogger<SendPushNotificationAtomicHandler> _logger;
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _appConfigs;

    public SendPushNotificationAtomicHandler(
        ILogger<SendPushNotificationAtomicHandler> logger,
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
    }

    /// <summary>
    /// Sends push notification to Buddy App microservice
    /// </summary>
    public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
    {
        SendPushNotificationHandlerReqDTO request = (SendPushNotificationHandlerReqDTO)downStreamRequestDTO;
        request.ValidateDownStreamRequestParameters();

        _logger.LogInformation("SendPushNotificationAtomicHandler: Starting to send push notification to microservice");

        // Validate configuration
        if (string.IsNullOrWhiteSpace(_appConfigs.MicroserviceBaseUrl))
        {
            _logger.LogError("SendPushNotificationAtomicHandler: Microservice base URL is not configured");
            throw new InvalidOperationException(ErrorConstants.SYS_NTFSVC_3001_MSG);
        }

        if (string.IsNullOrWhiteSpace(_appConfigs.ResourcePath))
        {
            _logger.LogError("SendPushNotificationAtomicHandler: Resource path is not configured");
            throw new InvalidOperationException(ErrorConstants.SYS_NTFSVC_3002_MSG);
        }

        // Build full URL
        string fullUrl = $"{_appConfigs.MicroserviceBaseUrl.TrimEnd('/')}/{_appConfigs.ResourcePath.TrimStart('/')}";
        _logger.LogInformation($"SendPushNotificationAtomicHandler: Target URL: {fullUrl}");

        // Create HTTP request message
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, fullUrl);

        // Add headers
        if (!string.IsNullOrWhiteSpace(request.OrganizationUnit))
        {
            httpRequestMessage.Headers.Add(InfoConstants.HEADER_ORGANIZATION_UNIT, request.OrganizationUnit);
        }

        if (!string.IsNullOrWhiteSpace(request.BusinessUnit))
        {
            httpRequestMessage.Headers.Add(InfoConstants.HEADER_BUSINESS_UNIT, request.BusinessUnit);
        }

        if (!string.IsNullOrWhiteSpace(request.Channel))
        {
            httpRequestMessage.Headers.Add(InfoConstants.HEADER_CHANNEL, request.Channel);
        }

        if (!string.IsNullOrWhiteSpace(request.AcceptLanguage))
        {
            httpRequestMessage.Headers.Add(InfoConstants.HEADER_ACCEPT_LANGUAGE, request.AcceptLanguage);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            httpRequestMessage.Headers.Add(InfoConstants.HEADER_SOURCE, request.Source);
        }

        // Set request body
        httpRequestMessage.Content = RestApiHelper.CreateJsonContent(request.NotificationPayload);

        _logger.LogInformation("SendPushNotificationAtomicHandler: Sending HTTP POST request to microservice");

        // Send HTTP request
        HttpResponseSnapshot httpResponseSnapshot = await _httpClient.SendAsync(httpRequestMessage);

        _logger.LogInformation($"SendPushNotificationAtomicHandler: Received response with status code: {httpResponseSnapshot.StatusCode}");

        return httpResponseSnapshot;
    }
}
