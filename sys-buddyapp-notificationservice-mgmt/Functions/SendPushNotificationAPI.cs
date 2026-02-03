using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Core.Extensions;
using Core.Exceptions;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Abstractions;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.SendPushNotificationDTO;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Constants;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Helpers;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Functions;

/// <summary>
/// Azure Function for sending push notifications to Buddy App drivers
/// HTTP-triggered function exposed to Process/Experience Layer
/// </summary>
public class SendPushNotificationAPI
{
    private readonly ILogger<SendPushNotificationAPI> _logger;
    private readonly INotificationMgmt _notificationMgmt;

    public SendPushNotificationAPI(
        ILogger<SendPushNotificationAPI> logger,
        INotificationMgmt notificationMgmt)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationMgmt = notificationMgmt ?? throw new ArgumentNullException(nameof(notificationMgmt));
    }

    /// <summary>
    /// Sends push notification to drivers
    /// </summary>
    /// <param name="req">HTTP request containing notification payload</param>
    /// <returns>HTTP response with notification result</returns>
    [Function("SendPushNotificationAPI")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications")] HttpRequestData req)
    {
        _logger.LogInformation("SendPushNotificationAPI: Function invoked");

        try
        {
            // Read request body
            string requestBody = await req.ReadAsStringAsync() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogError("SendPushNotificationAPI: Request body is null or empty");
                throw new NoRequestBodyException(ErrorConstants.SYS_NTFSVC_1001_MSG);
            }

            // Deserialize request
            SendPushNotificationReqDTO requestDto = RestApiHelper.DeserializeJsonResponse<SendPushNotificationReqDTO>(requestBody);

            _logger.LogInformation("SendPushNotificationAPI: Request deserialized successfully");

            // Extract headers
            Dictionary<string, string> headers = new();
            
            if (req.Headers.TryGetValues(InfoConstants.HEADER_ORGANIZATION_UNIT, out IEnumerable<string>? orgUnitValues))
            {
                headers[InfoConstants.HEADER_ORGANIZATION_UNIT] = orgUnitValues.FirstOrDefault() ?? string.Empty;
            }

            if (req.Headers.TryGetValues(InfoConstants.HEADER_BUSINESS_UNIT, out IEnumerable<string>? busUnitValues))
            {
                headers[InfoConstants.HEADER_BUSINESS_UNIT] = busUnitValues.FirstOrDefault() ?? string.Empty;
            }

            if (req.Headers.TryGetValues(InfoConstants.HEADER_CHANNEL, out IEnumerable<string>? channelValues))
            {
                headers[InfoConstants.HEADER_CHANNEL] = channelValues.FirstOrDefault() ?? string.Empty;
            }

            if (req.Headers.TryGetValues(InfoConstants.HEADER_ACCEPT_LANGUAGE, out IEnumerable<string>? acceptLangValues))
            {
                headers[InfoConstants.HEADER_ACCEPT_LANGUAGE] = acceptLangValues.FirstOrDefault() ?? string.Empty;
            }

            if (req.Headers.TryGetValues(InfoConstants.HEADER_SOURCE, out IEnumerable<string>? sourceValues))
            {
                headers[InfoConstants.HEADER_SOURCE] = sourceValues.FirstOrDefault() ?? string.Empty;
            }

            _logger.LogInformation("SendPushNotificationAPI: Headers extracted successfully");

            // Call service
            SendPushNotificationResDTO response = await _notificationMgmt.SendPushNotification(requestDto);

            _logger.LogInformation($"SendPushNotificationAPI: Notification processed with status: {response.Status}");

            // Create HTTP response
            HttpResponseData httpResponse = req.CreateResponse(HttpStatusCode.OK);
            await httpResponse.WriteAsJsonAsync(response);

            return httpResponse;
        }
        catch (NoRequestBodyException ex)
        {
            _logger.LogError($"SendPushNotificationAPI: No request body: {ex.Message}");
            throw;
        }
        catch (RequestValidationFailureException ex)
        {
            _logger.LogError($"SendPushNotificationAPI: Request validation failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"SendPushNotificationAPI: Unexpected error: {ex.Message}");
            throw new BaseException(ErrorConstants.SYS_NTFSVC_9001, ErrorConstants.SYS_NTFSVC_9001_MSG, ex);
        }
    }
}
