namespace CAFMSystemLayer.Constants
{
    public class ErrorConstants
    {
        // Authentication errors
        public static readonly (string ErrorCode, string Message) CAFM_AUTHENT_0001 = 
            ("CAFM_AUTHENT_0001", "Authentication to CAFM system failed.");
        public static readonly (string ErrorCode, string Message) CAFM_AUTHENT_0002 = 
            ("CAFM_AUTHENT_0002", "Authentication succeeded but no SessionId returned.");
        public static readonly (string ErrorCode, string Message) CAFM_LOGOUT_0001 = 
            ("CAFM_LOGOUT_0001", "Failed to logout from CAFM session.");
            
        // Location errors
        public static readonly (string ErrorCode, string Message) CAFM_LOCGET_0001 = 
            ("CAFM_LOCGET_0001", "Failed to get location details from CAFM.");
        public static readonly (string ErrorCode, string Message) CAFM_LOCGET_0002 = 
            ("CAFM_LOCGET_0002", "Location not found for given property/unit code.");
            
        // Instruction Set errors
        public static readonly (string ErrorCode, string Message) CAFM_INSGET_0001 = 
            ("CAFM_INSGET_0001", "Failed to get instruction sets from CAFM.");
        public static readonly (string ErrorCode, string Message) CAFM_INSGET_0002 = 
            ("CAFM_INSGET_0002", "Instruction set not found for given category.");
            
        // Task check errors
        public static readonly (string ErrorCode, string Message) CAFM_TSKCHK_0001 = 
            ("CAFM_TSKCHK_0001", "Failed to check existing tasks in CAFM.");
            
        // Task creation errors
        public static readonly (string ErrorCode, string Message) CAFM_TSKCRT_0001 = 
            ("CAFM_TSKCRT_0001", "Failed to create breakdown task in CAFM.");
        public static readonly (string ErrorCode, string Message) CAFM_TSKCRT_0002 = 
            ("CAFM_TSKCRT_0002", "Breakdown task already exists in CAFM.");
            
        // Event/Link errors
        public static readonly (string ErrorCode, string Message) CAFM_EVTCRT_0001 = 
            ("CAFM_EVTCRT_0001", "Failed to create event/link task in CAFM.");
    }
}
