namespace sys_oraclefusionhcm_mgmt.Constants;

/// <summary>
/// Error constants for Oracle Fusion HCM System Layer
/// Format: OFH_XXXXXX_DDDD where OFH = Oracle Fusion HCM (3 chars), XXXXXX = 6 chars, DDDD = 4 digits
/// </summary>
public static class ErrorConstants
{
    // CreateLeave API Errors (1000-1999)
    public const string OFH_CRLEAV_1001 = "OFH_CRLEAV_1001";
    public const string OFH_CRLEAV_1001_MSG = "Failed to create leave absence in Oracle Fusion HCM. Downstream API returned error.";
    
    public const string OFH_CRLEAV_1002 = "OFH_CRLEAV_1002";
    public const string OFH_CRLEAV_1002_MSG = "Invalid request payload for CreateLeave API. Required fields are missing or invalid.";
    
    public const string OFH_CRLEAV_1003 = "OFH_CRLEAV_1003";
    public const string OFH_CRLEAV_1003_MSG = "Oracle Fusion HCM API returned non-success HTTP status code.";
    
    public const string OFH_CRLEAV_1004 = "OFH_CRLEAV_1004";
    public const string OFH_CRLEAV_1004_MSG = "Failed to deserialize Oracle Fusion HCM API response.";
    
    // General System Layer Errors (9000-9999)
    public const string OFH_SYSTEM_9001 = "OFH_SYSTEM_9001";
    public const string OFH_SYSTEM_9001_MSG = "Unexpected error occurred in Oracle Fusion HCM System Layer.";
    
    public const string OFH_SYSTEM_9002 = "OFH_SYSTEM_9002";
    public const string OFH_SYSTEM_9002_MSG = "Configuration error: Oracle Fusion HCM connection settings are missing or invalid.";
    
    public const string OFH_SYSTEM_9003 = "OFH_SYSTEM_9003";
    public const string OFH_SYSTEM_9003_MSG = "Authentication failed: Invalid credentials for Oracle Fusion HCM API.";
}
