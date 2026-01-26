namespace CAFMSystem.Constants
{
    public class ErrorConstants
    {
        // Authentication errors
        public static readonly (string ErrorCode, string Message) SYS_AUTHENT_0001 = 
            ("SYS_AUTHENT_0001", "Authentication to CAFM system failed.");
        
        public static readonly (string ErrorCode, string Message) SYS_AUTHENT_0002 = 
            ("SYS_AUTHENT_0002", "Authentication succeeded but no SessionId returned.");
        
        public static readonly (string ErrorCode, string Message) SYS_LOGOUT_0001 = 
            ("SYS_LOGOUT_0001", "Failed to logout from CAFM session.");
        
        // Work Order operations errors
        public static readonly (string ErrorCode, string Message) SYS_WOCRT_0001 = 
            ("SYS_WOCRT_0001", "Failed to create work order in CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_WOCRT_0002 = 
            ("SYS_WOCRT_0002", "Work order already exists in CAFM system.");
        
        // Location errors
        public static readonly (string ErrorCode, string Message) SYS_LOCGET_0001 = 
            ("SYS_LOCGET_0001", "Failed to get location details from CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_LOCGET_0002 = 
            ("SYS_LOCGET_0002", "Location not found for given unit code.");
        
        // Instruction Set errors
        public static readonly (string ErrorCode, string Message) SYS_INSGET_0001 = 
            ("SYS_INSGET_0001", "Failed to get instruction sets from CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_INSGET_0002 = 
            ("SYS_INSGET_0002", "Instruction set not found for given sub-category.");
        
        // Breakdown Task errors
        public static readonly (string ErrorCode, string Message) SYS_TSKCRT_0001 = 
            ("SYS_TSKCRT_0001", "Failed to create breakdown task in CAFM system.");
        
        public static readonly (string ErrorCode, string Message) SYS_TSKGET_0001 = 
            ("SYS_TSKGET_0001", "Failed to get breakdown tasks from CAFM system.");
        
        // Event errors
        public static readonly (string ErrorCode, string Message) SYS_EVCRT_0001 = 
            ("SYS_EVCRT_0001", "Failed to create event in CAFM system.");
        
        // General errors
        public static readonly (string ErrorCode, string Message) SYS_GENRIC_0001 = 
            ("SYS_GENRIC_0001", "An unexpected error occurred while processing work order.");
    }
}
