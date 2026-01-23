namespace CAFMSystem.Constants
{
    public class ErrorConstants
    {
        // Authentication Errors
        public static readonly (string ErrorCode, string Message) SYS_AUTHENT_0001 = 
            ("SYS_AUTHENT_0001", "Authentication to CAFM system failed.");
        
        public static readonly (string ErrorCode, string Message) SYS_AUTHENT_0002 = 
            ("SYS_AUTHENT_0002", "Authentication succeeded but no SessionId returned.");
        
        public static readonly (string ErrorCode, string Message) SYS_LOGOUT_0001 = 
            ("SYS_LOGOUT_0001", "Failed to logout from CAFM session.");
        
        // GetLocationsByDto Errors
        public static readonly (string ErrorCode, string Message) SYS_LOCGET_0001 = 
            ("SYS_LOCGET_0001", "Failed to get location details from CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_LOCGET_0002 = 
            ("SYS_LOCGET_0002", "Location not found for given unit code.");
        
        // GetInstructionSetsByDto Errors
        public static readonly (string ErrorCode, string Message) SYS_INSGET_0001 = 
            ("SYS_INSGET_0001", "Failed to get instruction sets from CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_INSGET_0002 = 
            ("SYS_INSGET_0002", "Instruction set not found for given subcategory.");
        
        // GetBreakdownTasksByDto Errors
        public static readonly (string ErrorCode, string Message) SYS_TSKGET_0001 = 
            ("SYS_TSKGET_0001", "Failed to get breakdown tasks from CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_TSKGET_0002 = 
            ("SYS_TSKGET_0002", "No breakdown tasks found for given service request number.");
        
        // CreateBreakdownTask Errors
        public static readonly (string ErrorCode, string Message) SYS_TSKCRT_0001 = 
            ("SYS_TSKCRT_0001", "Failed to create breakdown task in CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_TSKCRT_0002 = 
            ("SYS_TSKCRT_0002", "Breakdown task creation returned no TaskId.");
        
        // CreateEvent Errors
        public static readonly (string ErrorCode, string Message) SYS_EVTCRT_0001 = 
            ("SYS_EVTCRT_0001", "Failed to create event in CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_EVTCRT_0002 = 
            ("SYS_EVTCRT_0002", "Event creation returned no EventId.");
    }
}
