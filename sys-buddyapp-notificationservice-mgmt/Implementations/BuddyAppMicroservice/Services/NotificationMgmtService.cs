using Microsoft.Extensions.Logging;
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
    public async Task<SendPushNotificationResDTO> SendPushNotification(SendPushNotificationReqDTO request)
    {
        _logger.LogInformation("NotificationMgmtService: SendPushNotification called");
        
        SendPushNotificationResDTO response = await _sendPushNotificationHandler.Handle(request);
        
        _logger.LogInformation($"NotificationMgmtService: SendPushNotification completed with status: {response.Status}");
        
        return response;
    }
}
