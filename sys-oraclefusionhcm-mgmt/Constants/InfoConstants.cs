namespace sys_oraclefusionhcm_mgmt.Constants;

/// <summary>
/// Informational constants for Oracle Fusion HCM System Layer
/// </summary>
public static class InfoConstants
{
    // CreateLeave API Info Messages
    public const string CREATE_LEAVE_SUCCESS = "Leave absence created successfully in Oracle Fusion HCM.";
    public const string CREATE_LEAVE_REQUEST_RECEIVED = "CreateLeave request received and processing.";
    public const string CREATE_LEAVE_CALLING_DOWNSTREAM = "Calling Oracle Fusion HCM API to create leave absence.";
    public const string CREATE_LEAVE_RESPONSE_RECEIVED = "Oracle Fusion HCM API response received successfully.";
    
    // General Info Messages
    public const string SYSTEM_LAYER_INITIALIZED = "Oracle Fusion HCM System Layer initialized successfully.";
    public const string DOWNSTREAM_API_CALL_START = "Starting downstream API call to Oracle Fusion HCM.";
    public const string DOWNSTREAM_API_CALL_COMPLETE = "Downstream API call to Oracle Fusion HCM completed.";
}
