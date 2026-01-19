namespace FacilitiesMgmtSystem.Constants;

/// <summary>
/// Error constants used throughout the application for consistent error messaging.
/// </summary>
public static class ErrorConstants
{
    // Validation Errors
    public const string INVALID_REQ_PAYLOAD = "Invalid request payload provided.";
    public const string SESSION_ID_NOT_FOUND_IN_CONTEXT = "Session ID not found in function context.";
    public const string MISSING_REQUIRED_FIELD = "Required field is missing: {0}";
    public const string INVALID_FIELD_VALUE = "Invalid value for field: {0}";

    // Authentication Errors
    public const string MRI_LOGIN_FAILED = "MRI login failed.";
    public const string MRI_LOGOUT_FAILED = "MRI logout failed.";
    public const string MRI_SESSION_EXPIRED = "MRI session has expired.";
    public const string MRI_AUTHENTICATION_REQUIRED = "MRI authentication is required for this operation.";

    // Downstream API Errors
    public const string DOWNSTREAM_API_FAILURE = "Downstream API call failed.";
    public const string SOAP_FAULT_RECEIVED = "SOAP fault received from downstream service.";
    public const string SOAP_RESPONSE_DESERIALIZATION_FAILED = "Failed to deserialize SOAP response.";
    public const string HTTP_REQUEST_FAILED = "HTTP request to downstream service failed.";

    // WorkOrder Errors
    public const string CREATE_WORKORDER_FAILED = "Failed to create work order.";
    public const string WORKORDER_VALIDATION_FAILED = "Work order validation failed.";
    public const string INVALID_WORKORDER_DATA = "Invalid work order data provided.";

    // BreakdownTask Errors
    public const string GET_BREAKDOWN_TASK_FAILED = "Failed to retrieve breakdown task.";
    public const string BREAKDOWN_TASK_NOT_FOUND = "Breakdown task not found.";

    // Location Errors
    public const string GET_LOCATION_FAILED = "Failed to retrieve location.";
    public const string LOCATION_NOT_FOUND = "Location not found.";

    // InstructionSets Errors
    public const string GET_INSTRUCTION_SETS_FAILED = "Failed to retrieve instruction sets.";
    public const string INSTRUCTION_SETS_NOT_FOUND = "Instruction sets not found.";

    // Configuration Errors
    public const string CONFIGURATION_MISSING = "Required configuration is missing: {0}";
    public const string SOAP_ENVELOPE_NOT_FOUND = "SOAP envelope template not found: {0}";
}
