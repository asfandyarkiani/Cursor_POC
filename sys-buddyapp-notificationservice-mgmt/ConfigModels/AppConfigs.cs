namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.ConfigModels;

/// <summary>
/// Application configuration model for Buddy App Notification Service
/// </summary>
public class AppConfigs
{
    /// <summary>
    /// Base URL for the microservice API
    /// Example: http://shared-ms.agp-dev.com
    /// </summary>
    public string MicroserviceBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for push notification endpoint
    /// Example: ms/comm/transaction
    /// </summary>
    public string ResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// HTTP request timeout in seconds
    /// Default: 30 seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for failed requests
    /// Default: 3 retries
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in seconds
    /// Default: 2 seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;
}
