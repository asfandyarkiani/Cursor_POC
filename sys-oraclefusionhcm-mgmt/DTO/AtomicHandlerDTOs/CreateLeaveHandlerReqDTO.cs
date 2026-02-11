using AlGhurair.Core.SystemLayer.DTOs;

namespace AlGhurair.SystemLayer.OracleFusionHCM.DTO.AtomicHandlerDTOs;

/// <summary>
/// Request DTO for Create Leave Atomic Handler
/// Represents transformed request to Oracle Fusion HCM API
/// </summary>
public class CreateLeaveHandlerReqDTO : IDownStreamRequestDTO
{
    /// <summary>
    /// Person number (mapped from EmployeeNumber)
    /// </summary>
    public string PersonNumber { get; set; } = string.Empty;

    /// <summary>
    /// Type of absence
    /// </summary>
    public string AbsenceType { get; set; } = string.Empty;

    /// <summary>
    /// Employer name
    /// </summary>
    public string Employer { get; set; } = string.Empty;

    /// <summary>
    /// Leave start date (YYYY-MM-DD format)
    /// </summary>
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// Leave end date (YYYY-MM-DD format)
    /// </summary>
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Absence status code (mapped from AbsenceStatusCode to AbsenceStatusCd)
    /// </summary>
    public string AbsenceStatusCd { get; set; } = string.Empty;

    /// <summary>
    /// Approval status code (mapped from ApprovalStatusCode to ApprovalStatusCd)
    /// </summary>
    public string ApprovalStatusCd { get; set; } = string.Empty;

    /// <summary>
    /// Duration on start date
    /// </summary>
    public decimal StartDateDuration { get; set; }

    /// <summary>
    /// Duration on end date
    /// </summary>
    public decimal EndDateDuration { get; set; }
}
