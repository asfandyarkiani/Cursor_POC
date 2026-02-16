namespace OracleFusionHCM.LeaveManagement.Constants
{
    public static class ErrorConstants
    {
        // Format: AAA_AAAAAA_DDDD
        // AAA = SOR abbreviation (3 chars): OFC (Oracle Fusion Cloud)
        // AAAAAA = Operation abbreviation (6 chars): LEVCRT (Leave Create)
        // DDDD = Error series number (4 digits): 0001, 0002, etc.
        
        public static readonly (string ErrorCode, string Message) OFC_LEVCRT_0001 = 
            ("OFC_LEVCRT_0001", "Failed to create leave entry in Oracle Fusion HCM");
        
        public static readonly (string ErrorCode, string Message) OFC_LEVCRT_0002 = 
            ("OFC_LEVCRT_0002", "Oracle Fusion HCM returned empty response body");
        
        public static readonly (string ErrorCode, string Message) OFC_LEVCRT_0003 = 
            ("OFC_LEVCRT_0003", "Oracle Fusion HCM authentication failed");
        
        public static readonly (string ErrorCode, string Message) OFC_LEVCRT_0004 = 
            ("OFC_LEVCRT_0004", "Invalid leave data provided");
        
        public static readonly (string ErrorCode, string Message) REQ_BODY_MISSING_OR_EMPTY = 
            ("REQ_BODY_MISSING_OR_EMPTY", "Request body is missing or empty");
    }
}
