namespace AGI.SysD365DriverLateLoginMgmt.Constants;

/// <summary>
/// Error constants for D365 Driver Late Login Management System Layer
/// Format: AAA_AAAAAA_DDDD where AAA = 3 chars, AAAAAA = 6 chars, DDDD = 4 digits
/// </summary>
public static class ErrorConstants
{
    // Authentication Errors (D365_AUTH_xxxx)
    public const string D365_AUTHEN_0001 = "D365_AUTHEN_0001: Failed to authenticate with D365 token endpoint";
    public const string D365_AUTHEN_0002 = "D365_AUTHEN_0002: Invalid or expired D365 authentication token";
    public const string D365_AUTHEN_0003 = "D365_AUTHEN_0003: Missing required authentication configuration (ClientId, ClientSecret, or TokenUrl)";

    // Late Login API Errors (D365_LATELOGIN_xxxx)
    public const string D365_LATLOG_0001 = "D365_LATLOG_0001: Failed to submit late login request to D365";
    public const string D365_LATLOG_0002 = "D365_LATLOG_0002: D365 late login API returned error response";
    public const string D365_LATLOG_0003 = "D365_LATLOG_0003: Invalid late login request data";
    public const string D365_LATLOG_0004 = "D365_LATLOG_0004: Missing required late login parameters (driverId, requestDateTime, or companyCode)";

    // Configuration Errors (D365_CONFIG_xxxx)
    public const string D365_CONFIG_0001 = "D365_CONFIG_0001: Missing or invalid D365 configuration";
    public const string D365_CONFIG_0002 = "D365_CONFIG_0002: Missing D365 BaseUrl configuration";
    public const string D365_CONFIG_0003 = "D365_CONFIG_0003: Missing D365 LateLoginResourcePath configuration";

    // Network/HTTP Errors (D365_NETWRK_xxxx)
    public const string D365_NETWRK_0001 = "D365_NETWRK_0001: Network timeout while calling D365 API";
    public const string D365_NETWRK_0002 = "D365_NETWRK_0002: D365 API returned HTTP error status code";
    public const string D365_NETWRK_0003 = "D365_NETWRK_0003: Failed to deserialize D365 API response";

    // Validation Errors (D365_VALIDN_xxxx)
    public const string D365_VALIDN_0001 = "D365_VALIDN_0001: Invalid driver ID format";
    public const string D365_VALIDN_0002 = "D365_VALIDN_0002: Invalid late login date/time format";
    public const string D365_VALIDN_0003 = "D365_VALIDN_0003: Invalid company code";
}
