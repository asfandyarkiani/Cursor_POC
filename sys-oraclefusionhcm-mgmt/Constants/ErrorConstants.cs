namespace OracleFusionHCMSystem.Constants
{
    public class ErrorConstants
    {
        // Format: AAA_AAAAAA_DDDD
        // AAA = SOR abbreviation (3 chars): HCM
        // AAAAAA = Operation abbreviation (6 chars): LEVCRT (Leave Create)
        // DDDD = Error series number (4 digits): 0001, 0002, etc.

        public static readonly (string ErrorCode, string Message) HCM_LEVCRT_0001 =
            ("HCM_LEVCRT_0001", "Failed to create leave absence entry in Oracle Fusion HCM.");

        public static readonly (string ErrorCode, string Message) HCM_LEVCRT_0002 =
            ("HCM_LEVCRT_0002", "Oracle Fusion HCM API returned empty response body.");

        public static readonly (string ErrorCode, string Message) HCM_LEVCRT_0003 =
            ("HCM_LEVCRT_0003", "Failed to decompress GZIP response from Oracle Fusion HCM.");
    }
}
