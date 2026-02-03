using Microsoft.Extensions.Logging;
using Core.SystemLayer.Middlewares;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Exceptions;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Constants;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.SendPushNotificationDTO;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.AtomicHandlerDTOs;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.DownstreamDTOs;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Implementations.BuddyAppMicroservice.AtomicHandlers;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Helpers;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Implementations.BuddyAppMicroservice.Handlers;

/// <summary>
/// Handler for sending push notifications
/// Orchestrates atomic handler and processes response
/// </summary>
public class SendPushNotificationHandler : IBaseHandler<SendPushNotificationReqDTO, SendPushNotificationResDTO>
{
    private readonly ILogger<SendPushNotificationHandler> _logger;
    private readonly SendPushNotificationAtomicHandler _atomicHandler;

    public SendPushNotificationHandler(
        ILogger<SendPushNotificationHandler> logger,
        SendPushNotificationAtomicHandler atomicHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _atomicHandler = atomicHandler ?? throw new ArgumentNullException(nameof(atomicHandler));
    }

    /// <summary>
    /// Handles push notification request
    /// </summary>
    public async Task<SendPushNotificationResDTO> Handle(SendPushNotificationReqDTO request, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("SendPushNotificationHandler: Starting to process push notification request");

        // Validate request
        if (!request.IsValid())
        {
            _logger.LogError("SendPushNotificationHandler: Invalid request");
            throw new RequestValidationFailureException(ErrorConstants.SYS_NTFSVC_1001_MSG);
        }

        // Extract headers
        string? organizationUnit = headers?.GetValueOrDefault(InfoConstants.HEADER_ORGANIZATION_UNIT);
        string? businessUnit = headers?.GetValueOrDefault(InfoConstants.HEADER_BUSINESS_UNIT);
        string? channel = headers?.GetValueOrDefault(InfoConstants.HEADER_CHANNEL);
        string? acceptLanguage = headers?.GetValueOrDefault(InfoConstants.HEADER_ACCEPT_LANGUAGE);
        string? source = headers?.GetValueOrDefault(InfoConstants.HEADER_SOURCE);

        // Build atomic handler request
        SendPushNotificationHandlerReqDTO atomicHandlerRequest = new()
        {
            NotificationPayload = request,
            OrganizationUnit = organizationUnit,
            BusinessUnit = businessUnit,
            Channel = channel,
            AcceptLanguage = acceptLanguage,
            Source = source
        };

        _logger.LogInformation("SendPushNotificationHandler: Calling atomic handler to send notification");

        // Call atomic handler
        HttpResponseSnapshot httpResponseSnapshot = await _atomicHandler.Handle(atomicHandlerRequest);

        _logger.LogInformation($"SendPushNotificationHandler: Received response with status code: {httpResponseSnapshot.StatusCode}");

        // Process response based on status code
        return ProcessResponse(httpResponseSnapshot);
    }

    /// <summary>
    /// Processes HTTP response from microservice
    /// Implements Boomi decision logic: status code 20x → check error message → return success/failure
    /// </summary>
    private SendPushNotificationResDTO ProcessResponse(HttpResponseSnapshot httpResponseSnapshot)
    {
        int statusCode = (int)httpResponseSnapshot.StatusCode;

        // Check if status code is 20x (success)
        if (RestApiHelper.IsSuccessStatusCode(statusCode))
        {
            _logger.LogInformation("SendPushNotificationHandler: Status code is 20x, checking response body for errors");

            // Deserialize response
            if (string.IsNullOrWhiteSpace(httpResponseSnapshot.Content))
            {
                _logger.LogWarning("SendPushNotificationHandler: Response content is empty");
                return SendPushNotificationResDTO.CreateSuccess(InfoConstants.NOTIFICATION_SENT_SUCCESS);
            }

            SendPushNotificationApiResDTO apiResponse = RestApiHelper.DeserializeJsonResponse<SendPushNotificationApiResDTO>(httpResponseSnapshot.Content);

            // Check if there are failed notifications with error messages
            if (apiResponse.Failed != null && apiResponse.Failed.Count > 0)
            {
                FailedNotification firstFailedNotification = apiResponse.Failed[0];
                string? errorMessage = firstFailedNotification.Error?.Response?.Message;

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    _logger.LogWarning($"SendPushNotificationHandler: Notification failed with error: {errorMessage}");
                    return SendPushNotificationResDTO.CreateFailure(errorMessage);
                }
            }

            // Success - extract notification ID from success array
            string successMessage = InfoConstants.NOTIFICATION_SENT_SUCCESS;
            if (apiResponse.Success != null && apiResponse.Success.Count > 0)
            {
                string? notificationId = apiResponse.Success[0]._id;
                if (!string.IsNullOrWhiteSpace(notificationId))
                {
                    successMessage = notificationId;
                }
            }

            _logger.LogInformation($"SendPushNotificationHandler: Notification sent successfully: {successMessage}");
            return SendPushNotificationResDTO.CreateSuccess(successMessage);
        }

        // Check if status code is 40x (client error)
        if (RestApiHelper.IsClientErrorStatusCode(statusCode))
        {
            _logger.LogWarning($"SendPushNotificationHandler: Status code is 40x: {statusCode}");

            string errorMessage = InfoConstants.DEFAULT_ERROR_MESSAGE;

            if (!string.IsNullOrWhiteSpace(httpResponseSnapshot.Content))
            {
                try
                {
                    BuddyAppStatus400ResDTO status400Response = RestApiHelper.DeserializeJsonResponse<BuddyAppStatus400ResDTO>(httpResponseSnapshot.Content);
                    
                    if (status400Response.Message != null && status400Response.Message.Count > 0)
                    {
                        errorMessage = status400Response.Message[0];
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"SendPushNotificationHandler: Failed to deserialize 400 error response: {ex.Message}");
                    errorMessage = httpResponseSnapshot.Content;
                }
            }

            _logger.LogError($"SendPushNotificationHandler: Client error: {errorMessage}");
            throw new DownStreamApiFailureException(ErrorConstants.SYS_NTFSVC_2003, ErrorConstants.SYS_NTFSVC_2003_MSG, errorMessage);
        }

        // Other error (5xx or other)
        _logger.LogError($"SendPushNotificationHandler: Unexpected status code: {statusCode}");

        string genericErrorMessage = RestApiHelper.ExtractErrorMessage(httpResponseSnapshot);
        throw new DownStreamApiFailureException(ErrorConstants.SYS_NTFSVC_2004, ErrorConstants.SYS_NTFSVC_2004_MSG, genericErrorMessage);
    }
}
