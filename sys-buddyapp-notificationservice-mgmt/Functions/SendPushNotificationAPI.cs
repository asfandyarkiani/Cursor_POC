using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using Core.DTOs;
using Core.Context;
using Core.Extensions;
using Core.Exceptions;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Abstractions;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.SendPushNotificationDTO;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Constants;

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
            // Store custom headers in Session context for Handler access
            StoreCustomHeaders(req);

            // Read and deserialize request body
            SendPushNotificationReqDTO? requestDto = await req.ReadBodyAsync<SendPushNotificationReqDTO>();

            if (requestDto == null)
            {
                _logger.LogError("SendPushNotificationAPI: Failed to deserialize request body");
                throw new NoRequestBodyException(ErrorConstants.SYS_NTFSVC_1001_MSG);
            }

            _logger.LogInformation("SendPushNotificationAPI: Request deserialized successfully");

            // Populate headers from HTTP request
            PopulateHeadersFromRequest(req, requestDto);

            // Call service
            BaseResponseDTO response = await _notificationMgmt.SendPushNotification(requestDto);

            _logger.LogInformation($"SendPushNotificationAPI: Notification processed with message: {response.Message}");

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

    /// <summary>
    /// Populates custom headers from HTTP request into DTO
    /// </summary>
    private void PopulateHeadersFromRequest(HttpRequestData req, SendPushNotificationReqDTO requestDto)
    {
        if (req.Headers.TryGetValues(InfoConstants.HEADER_ORGANIZATION_UNIT, out IEnumerable<string>? orgUnitValues))
        {
            requestDto.OrganizationUnit = orgUnitValues.FirstOrDefault();
        }

        if (req.Headers.TryGetValues(InfoConstants.HEADER_BUSINESS_UNIT, out IEnumerable<string>? busUnitValues))
        {
            requestDto.BusinessUnit = busUnitValues.FirstOrDefault();
        }

        if (req.Headers.TryGetValues(InfoConstants.HEADER_CHANNEL, out IEnumerable<string>? channelValues))
        {
            requestDto.Channel = channelValues.FirstOrDefault();
        }

        if (req.Headers.TryGetValues(InfoConstants.HEADER_ACCEPT_LANGUAGE, out IEnumerable<string>? acceptLangValues))
        {
            requestDto.AcceptLanguage = acceptLangValues.FirstOrDefault();
        }

        if (req.Headers.TryGetValues(InfoConstants.HEADER_SOURCE, out IEnumerable<string>? sourceValues))
        {
            requestDto.Source = sourceValues.FirstOrDefault();
        }

        _logger.LogInformation("SendPushNotificationAPI: Custom headers populated in request DTO");
    }
}
