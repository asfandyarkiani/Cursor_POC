using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Constants;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.SendPushNotificationDTO;

/// <summary>
/// Response DTO for push notification request
/// Maps to Boomi profile: Notification Response D365 (2f7ea94e-3063-493f-bc5e-8b0fe1bc0c66)
/// </summary>
public class SendPushNotificationResDTO
{
    /// <summary>
    /// Status of the notification request (success/failure)
    /// </summary>
    public string Status { get; set; } = InfoConstants.STATUS_FAILURE;

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = InfoConstants.DEFAULT_ERROR_MESSAGE;

    /// <summary>
    /// Creates a success response
    /// </summary>
    public static SendPushNotificationResDTO CreateSuccess(string message)
    {
        return new SendPushNotificationResDTO
        {
            Status = InfoConstants.STATUS_SUCCESS,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failure response
    /// </summary>
    public static SendPushNotificationResDTO CreateFailure(string message)
    {
        return new SendPushNotificationResDTO
        {
            Status = InfoConstants.STATUS_FAILURE,
            Message = message
        };
    }
}
