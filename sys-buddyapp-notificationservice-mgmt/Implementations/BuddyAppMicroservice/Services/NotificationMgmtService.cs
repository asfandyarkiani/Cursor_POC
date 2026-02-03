using Microsoft.Extensions.Logging;
using Core.DTOs;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Abstractions;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.DTO.SendPushNotificationDTO;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Implementations.BuddyAppMicroservice.Handlers;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Implementations.BuddyAppMicroservice.Services;

/// <summary>
/// Service implementation for Notification Management
/// Delegates to handlers for business operations
/// </summary>
public class NotificationMgmtService : INotificationMgmt
{
    private readonly ILogger<NotificationMgmtService> _logger;
    private readonly SendPushNotificationHandler _sendPushNotificationHandler;

    public NotificationMgmtService(
        ILogger<NotificationMgmtService> logger,
        SendPushNotificationHandler sendPushNotificationHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sendPushNotificationHandler = sendPushNotificationHandler ?? throw new ArgumentNullException(nameof(sendPushNotificationHandler));
    }

    /// <summary>
    /// Sends push notification to drivers
    /// </summary>
    public async Task<BaseResponseDTO> SendPushNotification(SendPushNotificationReqDTO request)
    {
        _logger.LogInformation("NotificationMgmtService: SendPushNotification called");
        
        BaseResponseDTO response = await _sendPushNotificationHandler.HandleAsync(request);
        
        _logger.LogInformation($"NotificationMgmtService: SendPushNotification completed with message: {response.Message}");
        
        return response;
    }
}
