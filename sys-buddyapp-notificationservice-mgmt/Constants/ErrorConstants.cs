namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Constants;

/// <summary>
/// Error constants for Buddy App Notification Service System Layer
/// Format: SYS_NTFSVC_DDDD where SYS = System Layer, NTFSVC = NotificationService (6 chars), DDDD = 4 digits
/// </summary>
public static class ErrorConstants
{
    // Validation Errors (1000-1999)
    public const string SYS_NTFSVC_1001 = "SYS_NTFSVC_1001";
    public const string SYS_NTFSVC_1001_MSG = "Request body is null or empty";

    public const string SYS_NTFSVC_1002 = "SYS_NTFSVC_1002";
    public const string SYS_NTFSVC_1002_MSG = "Notification data is required";

    public const string SYS_NTFSVC_1003 = "SYS_NTFSVC_1003";
    public const string SYS_NTFSVC_1003_MSG = "At least one notification mode is required";

    public const string SYS_NTFSVC_1004 = "SYS_NTFSVC_1004";
    public const string SYS_NTFSVC_1004_MSG = "Driver ID is required in notification data";

    public const string SYS_NTFSVC_1005 = "SYS_NTFSVC_1005";
    public const string SYS_NTFSVC_1005_MSG = "Notification title is required";

    public const string SYS_NTFSVC_1006 = "SYS_NTFSVC_1006";
    public const string SYS_NTFSVC_1006_MSG = "Notification message is required";

    public const string SYS_NTFSVC_1007 = "SYS_NTFSVC_1007";
    public const string SYS_NTFSVC_1007_MSG = "Mode type is required";

    public const string SYS_NTFSVC_1008 = "SYS_NTFSVC_1008";
    public const string SYS_NTFSVC_1008_MSG = "Mode provider is required";

    // Downstream API Errors (2000-2999)
    public const string SYS_NTFSVC_2001 = "SYS_NTFSVC_2001";
    public const string SYS_NTFSVC_2001_MSG = "Failed to send push notification to microservice";

    public const string SYS_NTFSVC_2002 = "SYS_NTFSVC_2002";
    public const string SYS_NTFSVC_2002_MSG = "Microservice returned error response";

    public const string SYS_NTFSVC_2003 = "SYS_NTFSVC_2003";
    public const string SYS_NTFSVC_2003_MSG = "Microservice returned 4xx client error";

    public const string SYS_NTFSVC_2004 = "SYS_NTFSVC_2004";
    public const string SYS_NTFSVC_2004_MSG = "Microservice returned 5xx server error";

    public const string SYS_NTFSVC_2005 = "SYS_NTFSVC_2005";
    public const string SYS_NTFSVC_2005_MSG = "Failed to deserialize microservice response";

    // Configuration Errors (3000-3999)
    public const string SYS_NTFSVC_3001 = "SYS_NTFSVC_3001";
    public const string SYS_NTFSVC_3001_MSG = "Microservice base URL is not configured";

    public const string SYS_NTFSVC_3002 = "SYS_NTFSVC_3002";
    public const string SYS_NTFSVC_3002_MSG = "Resource path is not configured";

    // General Errors (9000-9999)
    public const string SYS_NTFSVC_9001 = "SYS_NTFSVC_9001";
    public const string SYS_NTFSVC_9001_MSG = "Unexpected error occurred while processing notification request";
}
