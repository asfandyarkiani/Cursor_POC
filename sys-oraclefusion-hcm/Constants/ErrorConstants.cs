namespace OracleFusionHcm.Constants
{
    public class ErrorConstants
    {
        // Format: AAA_AAAAAA_DDDD
        // AAA = SOR abbreviation (3 chars): OFH (Oracle Fusion HCM)
        // AAAAAA = Operation abbreviation (6 chars)
        // DDDD = Error series number (4 digits)

        public static readonly (string ErrorCode, string Message) OFH_LEVCRT_0001 = ("OFH_LEVCRT_0001", "Failed to create leave in Oracle Fusion HCM.");
        public static readonly (string ErrorCode, string Message) OFH_LEVCRT_0002 = ("OFH_LEVCRT_0002", "Oracle Fusion HCM returned empty response body.");
        public static readonly (string ErrorCode, string Message) OFH_LEVCRT_0003 = ("OFH_LEVCRT_0003", "Failed to deserialize Oracle Fusion HCM response.");
    }
}
