namespace sys_oraclefusion_hcm.Constants
{
    public static class ErrorConstants
    {
        // Format: AAA_AAAAAA_DDDD
        // AAA = SOR abbreviation (3 chars): OFH (Oracle Fusion HCM)
        // AAAAAA = Operation abbreviation (6 chars)
        // DDDD = Error series number (4 digits)

        // Create Leave Errors
        public static readonly (string ErrorCode, string Message) OFH_LEVCRT_0001 =
            ("OFH_LEVCRT_0001", "Failed to create leave in Oracle Fusion HCM");

        public static readonly (string ErrorCode, string Message) OFH_LEVCRT_0002 =
            ("OFH_LEVCRT_0002", "Oracle Fusion HCM returned empty response for leave creation");

        public static readonly (string ErrorCode, string Message) OFH_LEVCRT_0003 =
            ("OFH_LEVCRT_0003", "Oracle Fusion HCM authentication failed");

        public static readonly (string ErrorCode, string Message) OFH_LEVCRT_0004 =
            ("OFH_LEVCRT_0004", "Invalid leave data provided to Oracle Fusion HCM");
    }
}
