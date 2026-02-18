namespace HcmLeaveProcessLayer.Constants
{
    public static class ErrorConstants
    {
        public static readonly (string ErrorCode, string Message) CREATE_LEAVE_FAILURE =
            ("HRM_CRTLEV_0001", "Failed to create leave in Oracle Fusion");
        
        public static readonly (string ErrorCode, string Message) SYSTEM_LAYER_CALL_FAILED =
            ("HRM_CRTLEV_0002", "System Layer API call failed");
    }
}
