namespace OracleFusionHcmMgmt.Constants
{
    /// <summary>
    /// Error constants for Oracle Fusion HCM System Layer.
    /// Format: AAA_AAAAAA_DDDD
    /// AAA = SOR abbreviation (3 chars): OFH (Oracle Fusion HCM)
    /// AAAAAA = Operation abbreviation (6 chars)
    /// DDDD = Error series number (4 digits)
    /// </summary>
    public class ErrorConstants
    {
        // Create Leave Operation Errors
        public static readonly (string ErrorCode, string Message) OFH_LVECRT_0001 = 
            ("OFH_LVECRT_0001", "Failed to create leave absence in Oracle Fusion HCM.");
        
        public static readonly (string ErrorCode, string Message) OFH_LVECRT_0002 = 
            ("OFH_LVECRT_0002", "Oracle Fusion HCM returned empty response body.");
        
        public static readonly (string ErrorCode, string Message) OFH_LVECRT_0003 = 
            ("OFH_LVECRT_0003", "Failed to parse Oracle Fusion HCM response.");
        
        // Email Notification Errors
        public static readonly (string ErrorCode, string Message) OFH_EMLERR_0001 = 
            ("OFH_EMLERR_0001", "Failed to send error notification email.");
        
        public static readonly (string ErrorCode, string Message) OFH_EMLERR_0002 = 
            ("OFH_EMLERR_0002", "Email service returned empty response.");
    }
}
