namespace CAFMSystem.Constants
{
    /// <summary>
    /// Error constants for CAFM System Layer operations.
    /// Format: AAA_AAAAAA_DDDD
    /// AAA = SOR abbreviation (3 chars): FSI (Facilities System International)
    /// AAAAAA = Operation abbreviation (6 chars): AUTHEN, TSKCRT, TSKGET, etc.
    /// DDDD = Error series number (4 digits): 0001, 0002, 0003
    /// </summary>
    public class ErrorConstants
    {
        // Authentication Errors
        public static readonly (string ErrorCode, string Message) FSI_AUTHEN_0001 = 
            ("FSI_AUTHEN_0001", "Authentication to CAFM system failed.");
        public static readonly (string ErrorCode, string Message) FSI_AUTHEN_0002 = 
            ("FSI_AUTHEN_0002", "CAFM authentication returned blank or invalid SessionId.");
        public static readonly (string ErrorCode, string Message) FSI_LOGOUT_0001 = 
            ("FSI_LOGOUT_0001", "Logout from CAFM system failed.");

        // GetBreakdownTasksByDto Errors
        public static readonly (string ErrorCode, string Message) FSI_TSKGET_0001 = 
            ("FSI_TSKGET_0001", "Failed to retrieve breakdown tasks from CAFM.");
        public static readonly (string ErrorCode, string Message) FSI_TSKGET_0002 = 
            ("FSI_TSKGET_0002", "CAFM returned invalid response for GetBreakdownTasksByDto.");

        // GetLocationsByDto Errors
        public static readonly (string ErrorCode, string Message) FSI_LOCGET_0001 = 
            ("FSI_LOCGET_0001", "Failed to retrieve locations from CAFM.");
        public static readonly (string ErrorCode, string Message) FSI_LOCGET_0002 = 
            ("FSI_LOCGET_0002", "CAFM returned no locations for the specified criteria.");

        // GetInstructionSetsByDto Errors
        public static readonly (string ErrorCode, string Message) FSI_INSTGT_0001 = 
            ("FSI_INSTGT_0001", "Failed to retrieve instruction sets from CAFM.");
        public static readonly (string ErrorCode, string Message) FSI_INSTGT_0002 = 
            ("FSI_INSTGT_0002", "CAFM returned no instruction sets for the specified criteria.");

        // CreateBreakdownTask Errors
        public static readonly (string ErrorCode, string Message) FSI_TSKCRT_0001 = 
            ("FSI_TSKCRT_0001", "Failed to create breakdown task in CAFM.");
        public static readonly (string ErrorCode, string Message) FSI_TSKCRT_0002 = 
            ("FSI_TSKCRT_0002", "CAFM returned blank or invalid TaskId after creation.");
        public static readonly (string ErrorCode, string Message) FSI_TSKCRT_0003 = 
            ("FSI_TSKCRT_0003", "CAFM CreateBreakdownTask returned partial success (50* status code).");

        // CreateEvent Errors
        public static readonly (string ErrorCode, string Message) FSI_EVTCRT_0001 = 
            ("FSI_EVTCRT_0001", "Failed to create recurring event in CAFM.");
        public static readonly (string ErrorCode, string Message) FSI_EVTCRT_0002 = 
            ("FSI_EVTCRT_0002", "CAFM returned invalid response for CreateEvent.");

        // Session Management Errors
        public static readonly (string ErrorCode, string Message) FSI_SESSIO_0001 = 
            ("FSI_SESSIO_0001", "SessionId not found in request context.");
        public static readonly (string ErrorCode, string Message) FSI_SESSIO_0002 = 
            ("FSI_SESSIO_0002", "SessionId is required for CAFM operations.");
    }
}
