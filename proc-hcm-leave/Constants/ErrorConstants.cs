namespace ProcHcmLeave.Constants
{
    public static class ErrorConstants
    {
        public static readonly (string ErrorCode, string Message) CREATE_LEAVE_FAILURE =
            ("HRM_CRTLVE_0001", "Failed to create leave in Oracle Fusion HCM");
        
        public static readonly (string ErrorCode, string Message) INVALID_REQUEST_DATA =
            ("HRM_CRTLVE_0002", "Invalid request data provided");
        
        public static readonly (string ErrorCode, string Message) SYSTEM_LAYER_CALL_FAILED =
            ("HRM_CRTLVE_0003", "System Layer call to Oracle Fusion HCM failed");
    }
}
