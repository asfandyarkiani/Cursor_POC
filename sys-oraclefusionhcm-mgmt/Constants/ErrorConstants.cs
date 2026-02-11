namespace AlGhurair.SystemLayer.OracleFusionHCM.Constants;

/// <summary>
/// Error constants for Oracle Fusion HCM System Layer
/// Format: OFH_LVEMGT_NNNN where OFH = OracleFusionHCM, LVEMGT = LeaveMgmt, NNNN = 4-digit code
/// </summary>
public static class ErrorConstants
{
    // Leave Management Errors (0001-0999)
    public const string OFH_LVEMGT_0001 = "OFH_LVEMGT_0001"; // Failed to create leave in Oracle Fusion HCM
    public const string OFH_LVEMGT_0002 = "OFH_LVEMGT_0002"; // Invalid leave request data
    public const string OFH_LVEMGT_0003 = "OFH_LVEMGT_0003"; // Oracle Fusion HCM API returned error
    public const string OFH_LVEMGT_0004 = "OFH_LVEMGT_0004"; // Failed to deserialize Oracle Fusion HCM response
    public const string OFH_LVEMGT_0005 = "OFH_LVEMGT_0005"; // Missing required configuration for Oracle Fusion HCM
    public const string OFH_LVEMGT_0006 = "OFH_LVEMGT_0006"; // Oracle Fusion HCM authentication failed
    public const string OFH_LVEMGT_0007 = "OFH_LVEMGT_0007"; // Oracle Fusion HCM connection timeout
    public const string OFH_LVEMGT_0008 = "OFH_LVEMGT_0008"; // Invalid employee number
    public const string OFH_LVEMGT_0009 = "OFH_LVEMGT_0009"; // Invalid date range for leave
    public const string OFH_LVEMGT_0010 = "OFH_LVEMGT_0010"; // Missing person absence entry ID in response
}
