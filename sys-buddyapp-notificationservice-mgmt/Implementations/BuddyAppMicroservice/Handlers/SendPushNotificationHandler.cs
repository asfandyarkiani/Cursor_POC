using Microsoft.Extensions.Logging;
using Core.DTOs;
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
public class SendPushNotificationHandler : IBaseHandler<SendPushNotificationReqDTO>
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
    public async Task<BaseResponseDTO> HandleAsync(SendPushNotificationReqDTO request)
    {
        _logger.LogInformation("SendPushNotificationHandler: Starting to process push notification request");

        // Validate request
        request.ValidateAPIRequestParameters();

        // Extract headers from request DTO (populated by Function)
        string? organizationUnit = request.OrganizationUnit;
        string? businessUnit = request.BusinessUnit;
        string? channel = request.Channel;
        string? acceptLanguage = request.AcceptLanguage;
        string? source = request.Source;

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
    private BaseResponseDTO ProcessResponse(HttpResponseSnapshot httpResponseSnapshot)
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
                SendPushNotificationResDTO successResponse = SendPushNotificationResDTO.CreateSuccess(InfoConstants.NOTIFICATION_SENT_SUCCESS);
                return new BaseResponseDTO(
                    message: InfoConstants.NOTIFICATION_SENT_SUCCESS,
                    errorCode: string.Empty,
                    data: successResponse
                );
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
                    SendPushNotificationResDTO failureResponse = SendPushNotificationResDTO.CreateFailure(errorMessage);
                    return new BaseResponseDTO(
                        message: errorMessage,
                        errorCode: ErrorConstants.SYS_NTFSVC_2002,
                        data: failureResponse
                    );
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
            SendPushNotificationResDTO successResponseDto = SendPushNotificationResDTO.CreateSuccess(successMessage);
            return new BaseResponseDTO(
                message: successMessage,
                errorCode: string.Empty,
                data: successResponseDto
            );
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
            throw new DownStreamApiFailureException(
                System.Net.HttpStatusCode.BadRequest,
                (ErrorConstants.SYS_NTFSVC_2003, ErrorConstants.SYS_NTFSVC_2003_MSG),
                new List<string> { errorMessage }
            );
        }

        // Other error (5xx or other)
        _logger.LogError($"SendPushNotificationHandler: Unexpected status code: {statusCode}");

        string genericErrorMessage = RestApiHelper.ExtractErrorMessage(httpResponseSnapshot);
        throw new DownStreamApiFailureException(
            (System.Net.HttpStatusCode)httpResponseSnapshot.StatusCode,
            (ErrorConstants.SYS_NTFSVC_2004, ErrorConstants.SYS_NTFSVC_2004_MSG),
            new List<string> { genericErrorMessage }
        );
    }
}
