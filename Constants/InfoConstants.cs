namespace FacilitiesMgmtSystem.Constants;

/// <summary>
/// Informational constants used throughout the application.
/// </summary>
public static class InfoConstants
{
    // Session Management
    public const string SESSION_ID = "SessionId";
    public const string MRI_SESSION_KEY = "MRI_SESSION";

    // SOAP Actions
    public const string SOAP_ACTION_CREATE_WORKORDER = "CreateWorkOrder";
    public const string SOAP_ACTION_GET_BREAKDOWN_TASK = "GetBreakdownTask";
    public const string SOAP_ACTION_GET_LOCATION = "GetLocation";
    public const string SOAP_ACTION_GET_INSTRUCTION_SETS = "GetInstructionSets";
    public const string SOAP_ACTION_LOGIN = "Login";
    public const string SOAP_ACTION_LOGOUT = "Logout";

    // SOAP Envelope Resource Names
    public const string SOAP_ENVELOPE_CREATE_WORKORDER = "FacilitiesMgmtSystem.SoapEnvelopes.CreateWorkOrder.xml";
    public const string SOAP_ENVELOPE_GET_BREAKDOWN_TASK = "FacilitiesMgmtSystem.SoapEnvelopes.GetBreakdownTask.xml";
    public const string SOAP_ENVELOPE_GET_LOCATION = "FacilitiesMgmtSystem.SoapEnvelopes.GetLocation.xml";
    public const string SOAP_ENVELOPE_GET_INSTRUCTION_SETS = "FacilitiesMgmtSystem.SoapEnvelopes.GetInstructionSets.xml";
    public const string SOAP_ENVELOPE_LOGIN = "FacilitiesMgmtSystem.SoapEnvelopes.MRILogin.xml";
    public const string SOAP_ENVELOPE_LOGOUT = "FacilitiesMgmtSystem.SoapEnvelopes.MRILogout.xml";

    // Logging Messages
    public const string REQUEST_RECEIVED = "HTTP request received for {0}.";
    public const string REQUEST_VALIDATED = "Request validation successful.";
    public const string PROCESSING_STARTED = "Processing started for {0}.";
    public const string PROCESSING_COMPLETED = "Processing completed for {0}.";
    public const string DOWNSTREAM_CALL_STARTED = "Starting downstream API call to {0}.";
    public const string DOWNSTREAM_CALL_COMPLETED = "Downstream API call completed for {0}.";

    // Content Types
    public const string CONTENT_TYPE_SOAP_XML = "text/xml; charset=utf-8";
    public const string CONTENT_TYPE_JSON = "application/json";
}
