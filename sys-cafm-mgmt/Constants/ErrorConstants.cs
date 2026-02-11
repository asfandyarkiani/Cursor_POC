namespace sys_cafm_mgmt.Constants
{
    public class ErrorConstants
    {
        // Format: AAA_AAAAAA_DDDD
        // AAA = SOR abbreviation (3 chars): CAF (CAFM)
        // AAAAAA = Operation abbreviation (6 chars)
        // DDDD = Error series number (4 digits)
        
        // Authentication Errors
        public static readonly (string ErrorCode, string Message) CAF_AUTHEN_0001 = 
            ("CAF_AUTHEN_0001", "Authentication to CAFM system failed.");
        public static readonly (string ErrorCode, string Message) CAF_AUTHEN_0002 = 
            ("CAF_AUTHEN_0002", "CAFM authentication returned empty SessionId.");
        public static readonly (string ErrorCode, string Message) CAF_LOGOUT_0001 = 
            ("CAF_LOGOUT_0001", "Logout from CAFM system failed.");
        
        // CreateBreakdownTask Errors
        public static readonly (string ErrorCode, string Message) CAF_TSKCRT_0001 = 
            ("CAF_TSKCRT_0001", "Failed to create breakdown task in CAFM system.");
        public static readonly (string ErrorCode, string Message) CAF_TSKCRT_0002 = 
            ("CAF_TSKCRT_0002", "CAFM CreateBreakdownTask returned empty response.");
        public static readonly (string ErrorCode, string Message) CAF_TSKCRT_0003 = 
            ("CAF_TSKCRT_0003", "Failed to retrieve location information from CAFM.");
        public static readonly (string ErrorCode, string Message) CAF_TSKCRT_0004 = 
            ("CAF_TSKCRT_0004", "Failed to retrieve instruction sets from CAFM.");
        public static readonly (string ErrorCode, string Message) CAF_TSKCRT_0005 = 
            ("CAF_TSKCRT_0005", "Failed to check existing breakdown tasks in CAFM.");
        
        // CreateEvent Errors
        public static readonly (string ErrorCode, string Message) CAF_EVTCRT_0001 = 
            ("CAF_EVTCRT_0001", "Failed to create event in CAFM system.");
        public static readonly (string ErrorCode, string Message) CAF_EVTCRT_0002 = 
            ("CAF_EVTCRT_0002", "CAFM CreateEvent returned empty response.");
        
        // Email Errors
        public static readonly (string ErrorCode, string Message) EML_SEND_0001 = 
            ("EML_SEND_0001", "Failed to send email notification.");
        public static readonly (string ErrorCode, string Message) EML_SEND_0002 = 
            ("EML_SEND_0002", "SMTP configuration is invalid.");
    }
}
