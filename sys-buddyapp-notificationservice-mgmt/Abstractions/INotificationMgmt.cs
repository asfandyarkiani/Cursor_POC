using Core.DTOs;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.SendPushNotificationDTO;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Abstractions;

/// <summary>
/// Interface for Notification Management operations
/// Declares all operations the business needs for push notifications
/// </summary>
public interface INotificationMgmt
{
    /// <summary>
    /// Sends push notification to drivers via Buddy App microservice
    /// </summary>
    /// <param name="request">Notification request containing modes and data</param>
    /// <returns>BaseResponseDTO with notification result</returns>
    Task<BaseResponseDTO> SendPushNotification(SendPushNotificationReqDTO request);
}
