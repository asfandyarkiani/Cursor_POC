namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.DownstreamDTOs;

/// <summary>
/// Response DTO from Buddy App microservice API
/// Maps to Boomi profile: Push Notifications BuddyApp Response (578dbe40-057d-4494-81d4-db1ff1526c75)
/// </summary>
public class SendPushNotificationApiResDTO
{
    /// <summary>
    /// Array of successfully sent notifications
    /// </summary>
    public List<SuccessNotification>? Success { get; set; }

    /// <summary>
    /// Array of failed notifications
    /// </summary>
    public List<FailedNotification>? Failed { get; set; }
}

/// <summary>
/// Successfully sent notification details
/// </summary>
public class SuccessNotification
{
    /// <summary>
    /// Notification type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Provider used
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Response from provider
    /// </summary>
    public string? Resp { get; set; }

    /// <summary>
    /// Reference ID
    /// </summary>
    public string? RefId { get; set; }

    /// <summary>
    /// Notification ID
    /// </summary>
    public string? _id { get; set; }
}

/// <summary>
/// Failed notification details
/// </summary>
public class FailedNotification
{
    /// <summary>
    /// Notification type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Provider used
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Error details
    /// </summary>
    public NotificationError? Error { get; set; }
}

/// <summary>
/// Notification error details
/// </summary>
public class NotificationError
{
    /// <summary>
    /// Error response details
    /// </summary>
    public ErrorResponse? Response { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Error options
    /// </summary>
    public object? Options { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Error name
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Error response from provider
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error message from provider
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Whether this is a validation error
    /// </summary>
    public bool? IsValidationError { get; set; }
}
