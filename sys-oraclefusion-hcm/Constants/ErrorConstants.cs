namespace OracleFusionHcmSystemLayer.Constants
{
    public class ErrorConstants
    {
        // Format: AAA_AAAAAA_DDDD
        // OFH = Oracle Fusion HCM (3 chars)
        // ABSCRT = Absence Create (6 chars)
        // DDDD = Error series number (4 digits)
        
        public static readonly (string ErrorCode, string Message) OFH_ABSCRT_0001 =
            ("OFH_ABSCRT_0001", "Failed to create absence in Oracle Fusion HCM.");
        
        public static readonly (string ErrorCode, string Message) OFH_ABSCRT_0002 =
            ("OFH_ABSCRT_0002", "Oracle Fusion HCM returned empty response body.");
        
        public static readonly (string ErrorCode, string Message) OFH_ABSCRT_0003 =
            ("OFH_ABSCRT_0003", "Failed to deserialize Oracle Fusion HCM response.");
    }
}
