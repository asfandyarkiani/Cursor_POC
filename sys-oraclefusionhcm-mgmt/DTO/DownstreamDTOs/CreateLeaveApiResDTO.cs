namespace AlGhurair.SystemLayer.OracleFusionHCM.DTO.DownstreamDTOs;

/// <summary>
/// Response DTO from Oracle Fusion HCM Create Leave API
/// Represents the complete response structure from Oracle HCM
/// </summary>
public class CreateLeaveApiResDTO
{
    /// <summary>
    /// Person absence entry ID (primary identifier of created leave record)
    /// </summary>
    public long PersonAbsenceEntryId { get; set; }

    /// <summary>
    /// Absence case ID
    /// </summary>
    public string? AbsenceCaseId { get; set; }

    /// <summary>
    /// Type of absence
    /// </summary>
    public string? AbsenceType { get; set; }

    /// <summary>
    /// Employer name
    /// </summary>
    public string? Employer { get; set; }

    /// <summary>
    /// Leave start date
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Leave end date
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Absence status code
    /// </summary>
    public string? AbsenceStatusCd { get; set; }

    /// <summary>
    /// Approval status code
    /// </summary>
    public string? ApprovalStatusCd { get; set; }

    /// <summary>
    /// Total leave duration
    /// </summary>
    public decimal? Duration { get; set; }

    /// <summary>
    /// Person number (employee number)
    /// </summary>
    public string? PersonNumber { get; set; }

    /// <summary>
    /// Start date duration
    /// </summary>
    public decimal? StartDateDuration { get; set; }

    /// <summary>
    /// End date duration
    /// </summary>
    public decimal? EndDateDuration { get; set; }

    /// <summary>
    /// Person ID
    /// </summary>
    public long? PersonId { get; set; }

    /// <summary>
    /// Absence type ID
    /// </summary>
    public long? AbsenceTypeId { get; set; }

    /// <summary>
    /// Created by
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public string? CreationDate { get; set; }

    /// <summary>
    /// Last updated by
    /// </summary>
    public string? LastUpdatedBy { get; set; }

    /// <summary>
    /// Last update date
    /// </summary>
    public string? LastUpdateDate { get; set; }

    /// <summary>
    /// Object version number
    /// </summary>
    public long? ObjectVersionNumber { get; set; }
}
