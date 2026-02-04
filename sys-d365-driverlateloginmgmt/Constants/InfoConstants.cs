namespace AGI.SysD365DriverLateLoginMgmt.Constants;

/// <summary>
/// Information constants for D365 Driver Late Login Management System Layer
/// </summary>
public static class InfoConstants
{
    // Authentication Info
    public const string AUTH_TOKEN_RETRIEVED = "D365 authentication token retrieved successfully";
    public const string AUTH_TOKEN_CACHED = "Using cached D365 authentication token";
    public const string AUTH_TOKEN_EXPIRED = "D365 authentication token expired, retrieving new token";

    // Late Login API Info
    public const string LATE_LOGIN_REQUEST_INITIATED = "Late login request initiated for driver";
    public const string LATE_LOGIN_REQUEST_SUCCESS = "Late login request submitted successfully to D365";
    public const string LATE_LOGIN_REQUEST_COMPLETED = "Late login request completed";

    // Validation Info
    public const string VALIDATION_STARTED = "Request validation started";
    public const string VALIDATION_COMPLETED = "Request validation completed successfully";

    // HTTP Info
    public const string HTTP_REQUEST_STARTED = "HTTP request to D365 API started";
    public const string HTTP_REQUEST_COMPLETED = "HTTP request to D365 API completed";
    public const string HTTP_RESPONSE_RECEIVED = "HTTP response received from D365 API";
}
