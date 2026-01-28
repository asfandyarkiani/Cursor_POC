namespace CAFMSystem.Constants
{
    public class ErrorConstants
    {
        // Format: AAA_AAAAAA_DDDD
        // AAA = SOR abbreviation (3 chars): CAF (CAFM)
        // AAAAAA = Operation abbreviation (6 chars)
        // DDDD = Error series number (4 digits)
        
        // Authentication errors
        public static readonly (string ErrorCode, string Message) CAF_AUTHEN_0001 = 
            ("CAF_AUTHEN_0001", "Authentication to CAFM system failed.");
        public static readonly (string ErrorCode, string Message) CAF_AUTHEN_0002 = 
            ("CAF_AUTHEN_0002", "CAFM authentication returned empty SessionId.");
        
        // Logout errors
        public static readonly (string ErrorCode, string Message) CAF_LOGOUT_0001 = 
            ("CAF_LOGOUT_0001", "Logout from CAFM system failed.");
        
        // GetBreakdownTasksByDto errors
        public static readonly (string ErrorCode, string Message) CAF_TSKGET_0001 = 
            ("CAF_TSKGET_0001", "Failed to get breakdown tasks from CAFM.");
        public static readonly (string ErrorCode, string Message) CAF_TSKGET_0002 = 
            ("CAF_TSKGET_0002", "CAFM returned empty response for breakdown tasks query.");
        
        // GetLocationsByDto errors
        public static readonly (string ErrorCode, string Message) CAF_LOCGET_0001 = 
            ("CAF_LOCGET_0001", "Failed to get locations from CAFM.");
        public static readonly (string ErrorCode, string Message) CAF_LOCGET_0002 = 
            ("CAF_LOCGET_0002", "CAFM returned empty response for locations query.");
        
        // GetInstructionSetsByDto errors
        public static readonly (string ErrorCode, string Message) CAF_INSTGT_0001 = 
            ("CAF_INSTGT_0001", "Failed to get instruction sets from CAFM.");
        public static readonly (string ErrorCode, string Message) CAF_INSTGT_0002 = 
            ("CAF_INSTGT_0002", "CAFM returned empty response for instruction sets query.");
        
        // CreateBreakdownTask errors
        public static readonly (string ErrorCode, string Message) CAF_TSKCRT_0001 = 
            ("CAF_TSKCRT_0001", "Failed to create breakdown task in CAFM.");
        public static readonly (string ErrorCode, string Message) CAF_TSKCRT_0002 = 
            ("CAF_TSKCRT_0002", "CAFM returned empty response for breakdown task creation.");
        
        // CreateEvent errors
        public static readonly (string ErrorCode, string Message) CAF_EVTCRT_0001 = 
            ("CAF_EVTCRT_0001", "Failed to create event in CAFM.");
        public static readonly (string ErrorCode, string Message) CAF_EVTCRT_0002 = 
            ("CAF_EVTCRT_0002", "CAFM returned empty response for event creation.");
    }
}
