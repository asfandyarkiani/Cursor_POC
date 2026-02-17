using Core.SystemLayer.DTOs;
using System.ComponentModel.DataAnnotations;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Constants;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.SendPushNotificationDTO;

/// <summary>
/// Request DTO for sending push notifications
/// Maps to Boomi profile: Notifications Request D365 (73bddad7-d222-4774-ad8a-6efe0b9feac6)
/// </summary>
public class SendPushNotificationReqDTO : IRequestSysDTO
{
    /// <summary>
    /// Array of notification modes (push, sms, email, etc.)
    /// </summary>
    [Required(ErrorMessage = ErrorConstants.SYS_NTFSVC_1003_MSG)]
    [MinLength(1, ErrorMessage = ErrorConstants.SYS_NTFSVC_1003_MSG)]
    public List<NotificationMode> Modes { get; set; } = new();

    /// <summary>
    /// Notification data containing driver information and message details
    /// </summary>
    [Required(ErrorMessage = ErrorConstants.SYS_NTFSVC_1002_MSG)]
    public NotificationData Data { get; set; } = new();

    // Headers (populated by Function, not part of JSON body)
    /// <summary>
    /// Organization unit header (not serialized)
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? OrganizationUnit { get; set; }

    /// <summary>
    /// Business unit header (not serialized)
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? BusinessUnit { get; set; }

    /// <summary>
    /// Channel header (not serialized)
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? Channel { get; set; }

    /// <summary>
    /// Accept-Language header (not serialized)
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? AcceptLanguage { get; set; }

    /// <summary>
    /// Source header (not serialized)
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? Source { get; set; }

    /// <summary>
    /// Validates the request DTO
    /// Throws RequestValidationFailureException if validation fails
    /// </summary>
    public void ValidateAPIRequestParameters()
    {
        if (Modes == null || Modes.Count == 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException((ErrorConstants.SYS_NTFSVC_1003, ErrorConstants.SYS_NTFSVC_1003_MSG));
        }

        if (Data == null)
        {
            throw new Core.Exceptions.RequestValidationFailureException((ErrorConstants.SYS_NTFSVC_1002, ErrorConstants.SYS_NTFSVC_1002_MSG));
        }

        if (string.IsNullOrWhiteSpace(Data.DriverId))
        {
            throw new Core.Exceptions.RequestValidationFailureException((ErrorConstants.SYS_NTFSVC_1004, ErrorConstants.SYS_NTFSVC_1004_MSG));
        }

        if (string.IsNullOrWhiteSpace(Data.Title))
        {
            throw new Core.Exceptions.RequestValidationFailureException((ErrorConstants.SYS_NTFSVC_1005, ErrorConstants.SYS_NTFSVC_1005_MSG));
        }

        if (string.IsNullOrWhiteSpace(Data.Message))
        {
            throw new Core.Exceptions.RequestValidationFailureException((ErrorConstants.SYS_NTFSVC_1006, ErrorConstants.SYS_NTFSVC_1006_MSG));
        }

        // Validate each mode
        foreach (NotificationMode mode in Modes)
        {
            if (string.IsNullOrWhiteSpace(mode.Type))
            {
                throw new Core.Exceptions.RequestValidationFailureException((ErrorConstants.SYS_NTFSVC_1007, ErrorConstants.SYS_NTFSVC_1007_MSG));
            }

            if (string.IsNullOrWhiteSpace(mode.Provider))
            {
                throw new Core.Exceptions.RequestValidationFailureException((ErrorConstants.SYS_NTFSVC_1008, ErrorConstants.SYS_NTFSVC_1008_MSG));
            }
        }
    }
}

/// <summary>
/// Notification mode configuration
/// </summary>
public class NotificationMode
{
    /// <summary>
    /// Type of notification (e.g., "push", "sms", "email")
    /// </summary>
    [Required(ErrorMessage = ErrorConstants.SYS_NTFSVC_1007_MSG)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Provider for the notification (e.g., "fcm", "twilio", "sendgrid")
    /// </summary>
    [Required(ErrorMessage = ErrorConstants.SYS_NTFSVC_1008_MSG)]
    public string Provider { get; set; } = string.Empty;
}

/// <summary>
/// Notification data containing message and driver information
/// </summary>
public class NotificationData
{
    /// <summary>
    /// Driver ID to send notification to
    /// </summary>
    [Required(ErrorMessage = ErrorConstants.SYS_NTFSVC_1004_MSG)]
    public string DriverId { get; set; } = string.Empty;

    /// <summary>
    /// Notification title
    /// </summary>
    [Required(ErrorMessage = ErrorConstants.SYS_NTFSVC_1005_MSG)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message body
    /// </summary>
    [Required(ErrorMessage = ErrorConstants.SYS_NTFSVC_1006_MSG)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional data payload (optional)
    /// </summary>
    public NotificationDataPayload? Data { get; set; }
}

/// <summary>
/// Additional notification data payload
/// </summary>
public class NotificationDataPayload
{
    /// <summary>
    /// Custom field foo
    /// </summary>
    public string? Foo { get; set; }

    /// <summary>
    /// Custom field bar
    /// </summary>
    public string? Bar { get; set; }
}
