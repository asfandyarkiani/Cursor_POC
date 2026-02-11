namespace CAFMManagementSystem.Constants
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
        
        // GetBreakdownTasksByDto Errors
        public static readonly (string ErrorCode, string Message) CAF_GETTSK_0001 = 
            ("CAF_GETTSK_0001", "Failed to get breakdown tasks from CAFM system.");
        public static readonly (string ErrorCode, string Message) CAF_GETTSK_0002 = 
            ("CAF_GETTSK_0002", "CAFM GetBreakdownTasksByDto returned empty response.");
        
        // CreateBreakdownTask Errors
        public static readonly (string ErrorCode, string Message) CAF_CRTTSK_0001 = 
            ("CAF_CRTTSK_0001", "Failed to create breakdown task in CAFM system.");
        public static readonly (string ErrorCode, string Message) CAF_CRTTSK_0002 = 
            ("CAF_CRTTSK_0002", "CAFM CreateBreakdownTask returned empty response.");
        public static readonly (string ErrorCode, string Message) CAF_CRTTSK_0003 = 
            ("CAF_CRTTSK_0003", "CAFM CreateBreakdownTask returned error status.");
        
        // GetLocationsByDto Errors
        public static readonly (string ErrorCode, string Message) CAF_GETLOC_0001 = 
            ("CAF_GETLOC_0001", "Failed to get locations from CAFM system.");
        public static readonly (string ErrorCode, string Message) CAF_GETLOC_0002 = 
            ("CAF_GETLOC_0002", "CAFM GetLocationsByDto returned empty response.");
        
        // GetInstructionSetsByDto Errors
        public static readonly (string ErrorCode, string Message) CAF_GETINS_0001 = 
            ("CAF_GETINS_0001", "Failed to get instruction sets from CAFM system.");
        public static readonly (string ErrorCode, string Message) CAF_GETINS_0002 = 
            ("CAF_GETINS_0002", "CAFM GetInstructionSetsByDto returned empty response.");
        
        // CreateEvent Errors
        public static readonly (string ErrorCode, string Message) CAF_CRTEVT_0001 = 
            ("CAF_CRTEVT_0001", "Failed to create event in CAFM system.");
        public static readonly (string ErrorCode, string Message) CAF_CRTEVT_0002 = 
            ("CAF_CRTEVT_0002", "CAFM CreateEvent returned empty response.");
        
        // Lookup Errors
        public static readonly (string ErrorCode, string Message) CAF_LOOKUP_0001 = 
            ("CAF_LOOKUP_0001", "Failed to get lookup data from CAFM system.");
        public static readonly (string ErrorCode, string Message) CAF_LOOKUP_0002 = 
            ("CAF_LOOKUP_0002", "CAFM lookup operation returned empty response.");
        
        // Session Errors
        public static readonly (string ErrorCode, string Message) CAF_SESSIO_0001 = 
            ("CAF_SESSIO_0001", "SessionId not found in RequestContext.");
        public static readonly (string ErrorCode, string Message) CAF_SESSIO_0002 = 
            ("CAF_SESSIO_0002", "SessionId is required for CAFM operations.");
    }
}
