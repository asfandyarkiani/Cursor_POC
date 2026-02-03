using Core.SystemLayer.DTOs;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.AtomicHandlerDTOs;

/// <summary>
/// Request DTO for SendPushNotificationAtomicHandler
/// Contains the notification payload and headers for downstream API call
/// </summary>
public class SendPushNotificationHandlerReqDTO : IDownStreamRequestDTO
{
    /// <summary>
    /// Notification payload to send to microservice
    /// </summary>
    public object NotificationPayload { get; set; } = new();

    /// <summary>
    /// Organization unit header value
    /// </summary>
    public string? OrganizationUnit { get; set; }

    /// <summary>
    /// Business unit header value
    /// </summary>
    public string? BusinessUnit { get; set; }

    /// <summary>
    /// Channel header value
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Accept-Language header value
    /// </summary>
    public string? AcceptLanguage { get; set; }

    /// <summary>
    /// Source system header value
    /// </summary>
    public string? Source { get; set; }
}
