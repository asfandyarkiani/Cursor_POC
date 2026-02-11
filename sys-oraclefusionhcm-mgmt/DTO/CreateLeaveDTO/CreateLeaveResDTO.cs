namespace AlGhurair.SystemLayer.OracleFusionHCM.DTO.CreateLeaveDTO;

/// <summary>
/// Response DTO for Create Leave API
/// Represents leave creation response to D365 or Process Layer
/// </summary>
public class CreateLeaveResDTO
{
    /// <summary>
    /// Status of the operation ("success" or "failure")
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Person absence entry ID from Oracle Fusion HCM (null on error)
    /// </summary>
    public long? PersonAbsenceEntryId { get; set; }

    /// <summary>
    /// Success flag ("true" or "false")
    /// </summary>
    public string Success { get; set; } = string.Empty;
}
