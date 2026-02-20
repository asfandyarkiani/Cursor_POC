namespace ProcHcmLeaveCreate.Constants
{
    public static class ErrorConstants
    {
        public static readonly (string ErrorCode, string Message) CREATE_LEAVE_FAILURE =
            ("HRM_CRTLVE_0001", "Failed to create leave in Oracle Fusion HCM");
    }
}
