namespace FacilitiesMgmtSystem.Constants;

/// <summary>
/// Constants for error messages and error codes.
/// </summary>
public static class ErrorConstants
{
    public const string INVALID_REQ_PAYLOAD = "Invalid request payload";
    public const string SESSION_ID_NOT_FOUND_IN_CONTEXT = "Session ID not found in context";
    public const string DOWNSTREAM_API_FAILURE = "Downstream API call failed";
    public const string SOAP_ENVELOPE_NOT_FOUND = "SOAP envelope template not found";
    public const string SOAP_DESERIALIZATION_FAILED = "Failed to deserialize SOAP response";
    public const string INTERNAL_SERVER_ERROR = "Internal server error";
    public const string UNAUTHORIZED = "Unauthorized";
    public const string MRI_AUTHENTICATION_FAILED = "MRI authentication failed";
}
