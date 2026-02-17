namespace ProcHcmLeave.Constants
{
    public static class ErrorConstants
    {
        // HRM = HumanResource (3 chars)
        // CRTLVE = Create Leave (6 chars)
        // Format: AAA_AAAAAA_DDDD (BusinessDomain_Operation_Number)
        
        public static readonly (string ErrorCode, string Message) CREATE_LEAVE_FAILURE =
            ("HRM_CRTLVE_0001", "Failed to create leave in Oracle Fusion HCM");
        
        public static readonly (string ErrorCode, string Message) SYSTEM_LAYER_CALL_FAILURE =
            ("HRM_CRTLVE_0002", "System Layer call to CreateAbsence failed");
        
        public static readonly (string ErrorCode, string Message) UNKNOWN_ERROR =
            ("HRM_CRTLVE_0003", "An unknown error occurred while creating leave");
    }
}
