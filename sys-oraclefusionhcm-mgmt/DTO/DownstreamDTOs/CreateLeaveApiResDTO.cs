namespace sys_oraclefusionhcm_mgmt.DTO.DownstreamDTOs;

/// <summary>
/// Response DTO from Oracle Fusion HCM absences API
/// Represents the JSON response returned by Oracle Fusion HCM when creating a leave absence
/// </summary>
public class CreateLeaveApiResDTO
{
    /// <summary>
    /// Person absence entry ID (unique identifier for the created absence)
    /// </summary>
    public long PersonAbsenceEntryId { get; set; }
    
    /// <summary>
    /// Absence type
    /// </summary>
    public string? AbsenceType { get; set; }
    
    /// <summary>
    /// Employer name
    /// </summary>
    public string? Employer { get; set; }
    
    /// <summary>
    /// Start date
    /// </summary>
    public string? StartDate { get; set; }
    
    /// <summary>
    /// End date
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
    /// Start date duration
    /// </summary>
    public decimal? StartDateDuration { get; set; }
    
    /// <summary>
    /// End date duration
    /// </summary>
    public decimal? EndDateDuration { get; set; }
    
    /// <summary>
    /// Person number
    /// </summary>
    public string? PersonNumber { get; set; }
    
    /// <summary>
    /// Total duration
    /// </summary>
    public double? Duration { get; set; }
    
    /// <summary>
    /// Created by user
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Creation date
    /// </summary>
    public string? CreationDate { get; set; }
    
    /// <summary>
    /// Last update date
    /// </summary>
    public string? LastUpdateDate { get; set; }
    
    /// <summary>
    /// Last updated by user
    /// </summary>
    public string? LastUpdatedBy { get; set; }
    
    /// <summary>
    /// Person ID
    /// </summary>
    public long? PersonId { get; set; }
    
    /// <summary>
    /// Absence type ID
    /// </summary>
    public long? AbsenceTypeId { get; set; }
    
    /// <summary>
    /// Legal entity ID
    /// </summary>
    public long? LegalEntityId { get; set; }
    
    /// <summary>
    /// Legislation code
    /// </summary>
    public string? LegislationCode { get; set; }
    
    /// <summary>
    /// Object version number
    /// </summary>
    public long? ObjectVersionNumber { get; set; }
}
