namespace OracleFusionHCMSystemLayer.Constants
{
    public class ErrorConstants
    {
        // Format: AAA_AAAAAA_DDDD
        // AAA = SOR abbreviation (3 chars): OFH = Oracle Fusion HCM
        // AAAAAA = Operation abbreviation (6 chars)
        // DDDD = Error series number (4 digits)

        // Create Absence Errors
        public static readonly (string ErrorCode, string Message) OFH_ABSCRT_0001 =
            ("OFH_ABSCRT_0001", "Failed to create absence entry in Oracle Fusion HCM");

        public static readonly (string ErrorCode, string Message) OFH_ABSCRT_0002 =
            ("OFH_ABSCRT_0002", "Oracle Fusion HCM returned an error response");

        public static readonly (string ErrorCode, string Message) OFH_ABSCRT_0003 =
            ("OFH_ABSCRT_0003", "Failed to decompress gzip response from Oracle Fusion HCM");

        public static readonly (string ErrorCode, string Message) OFH_ABSCRT_0004 =
            ("OFH_ABSCRT_0004", "No response body received from Oracle Fusion HCM");

        // Authentication Errors
        public static readonly (string ErrorCode, string Message) OFH_AUTHEN_0001 =
            ("OFH_AUTHEN_0001", "Failed to retrieve credentials from KeyVault");

        // General Errors
        public static readonly (string ErrorCode, string Message) OFH_GENRIC_0001 =
            ("OFH_GENRIC_0001", "An unexpected error occurred while processing the request");
    }
}
