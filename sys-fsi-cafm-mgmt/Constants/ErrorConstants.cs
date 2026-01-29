namespace FsiCafmSystem.Constants
{
    public class ErrorConstants
    {
        // Authentication Errors
        public static readonly (string ErrorCode, string Message) FSI_AUTHENT_0001 = 
            ("FSI_AUTHENT_0001", "Authentication to FSI CAFM system failed.");
        public static readonly (string ErrorCode, string Message) FSI_AUTHENT_0002 = 
            ("FSI_AUTHENT_0002", "Authentication succeeded but no SessionId returned.");
        public static readonly (string ErrorCode, string Message) FSI_LOGOUT_0001 = 
            ("FSI_LOGOUT_0001", "Failed to logout from FSI CAFM session.");
        
        // Work Order Operations
        public static readonly (string ErrorCode, string Message) FSI_WOCRT_0001 = 
            ("FSI_WOCRT_0001", "Failed to create work order in FSI CAFM system.");
        public static readonly (string ErrorCode, string Message) FSI_WOCRT_0002 = 
            ("FSI_WOCRT_0002", "Work order creation succeeded but no TaskId returned.");
        public static readonly (string ErrorCode, string Message) FSI_WOCRT_0003 = 
            ("FSI_WOCRT_0003", "No response from FSI CAFM API after timeout.");
        
        // Location Operations
        public static readonly (string ErrorCode, string Message) FSI_LOCGET_0001 = 
            ("FSI_LOCGET_0001", "Failed to get location details from FSI CAFM system.");
        public static readonly (string ErrorCode, string Message) FSI_LOCGET_0002 = 
            ("FSI_LOCGET_0002", "Location not found for given unit code.");
        
        // Instruction Sets Operations
        public static readonly (string ErrorCode, string Message) FSI_INSGET_0001 = 
            ("FSI_INSGET_0001", "Failed to get instruction sets from FSI CAFM system.");
        public static readonly (string ErrorCode, string Message) FSI_INSGET_0002 = 
            ("FSI_INSGET_0002", "Instruction set not found for given description.");
        
        // Task Operations
        public static readonly (string ErrorCode, string Message) FSI_TSKGET_0001 = 
            ("FSI_TSKGET_0001", "Failed to get breakdown tasks from FSI CAFM system.");
        public static readonly (string ErrorCode, string Message) FSI_TSKCRT_0001 = 
            ("FSI_TSKCRT_0001", "Failed to create breakdown task in FSI CAFM system.");
        public static readonly (string ErrorCode, string Message) FSI_EVTCRT_0001 = 
            ("FSI_EVTCRT_0001", "Failed to create event in FSI CAFM system.");
        
        // Email Operations
        public static readonly (string ErrorCode, string Message) EML_SEND_0001 = 
            ("EML_SEND_0001", "Failed to send email.");
        public static readonly (string ErrorCode, string Message) EML_SEND_0002 = 
            ("EML_SEND_0002", "SMTP connection failed.");
    }
}
