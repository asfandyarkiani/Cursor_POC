using Core.SystemLayer.DTOs;
using System.ComponentModel.DataAnnotations;

namespace sys_oraclefusionhcm_mgmt.DTO.CreateLeaveDTO;

/// <summary>
/// Request DTO for CreateLeave API
/// Represents the input payload from Process Layer for creating leave absence in Oracle Fusion HCM
/// </summary>
public class CreateLeaveReqDTO : IRequestSysDTO
{
    /// <summary>
    /// Employee number/identifier
    /// </summary>
    [Required(ErrorMessage = "EmployeeNumber is required")]
    public int EmployeeNumber { get; set; }
    
    /// <summary>
    /// Type of absence/leave (e.g., "Sick Leave", "Annual Leave")
    /// </summary>
    [Required(ErrorMessage = "AbsenceType is required")]
    [StringLength(100, ErrorMessage = "AbsenceType cannot exceed 100 characters")]
    public string AbsenceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Employer name
    /// </summary>
    [Required(ErrorMessage = "Employer is required")]
    [StringLength(200, ErrorMessage = "Employer cannot exceed 200 characters")]
    public string Employer { get; set; } = string.Empty;
    
    /// <summary>
    /// Leave start date (format: YYYY-MM-DD)
    /// </summary>
    [Required(ErrorMessage = "StartDate is required")]
    public string StartDate { get; set; } = string.Empty;
    
    /// <summary>
    /// Leave end date (format: YYYY-MM-DD)
    /// </summary>
    [Required(ErrorMessage = "EndDate is required")]
    public string EndDate { get; set; } = string.Empty;
    
    /// <summary>
    /// Absence status code (e.g., "SUBMITTED", "APPROVED")
    /// </summary>
    [Required(ErrorMessage = "AbsenceStatusCode is required")]
    [StringLength(50, ErrorMessage = "AbsenceStatusCode cannot exceed 50 characters")]
    public string AbsenceStatusCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Approval status code (e.g., "APPROVED", "PENDING")
    /// </summary>
    [Required(ErrorMessage = "ApprovalStatusCode is required")]
    [StringLength(50, ErrorMessage = "ApprovalStatusCode cannot exceed 50 characters")]
    public string ApprovalStatusCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Duration on start date (in days, e.g., 1 for full day, 0.5 for half day)
    /// </summary>
    [Required(ErrorMessage = "StartDateDuration is required")]
    [Range(0, 1, ErrorMessage = "StartDateDuration must be between 0 and 1")]
    public decimal StartDateDuration { get; set; }
    
    /// <summary>
    /// Duration on end date (in days, e.g., 1 for full day, 0.5 for half day)
    /// </summary>
    [Required(ErrorMessage = "EndDateDuration is required")]
    [Range(0, 1, ErrorMessage = "EndDateDuration must be between 0 and 1")]
    public decimal EndDateDuration { get; set; }
    
    /// <summary>
    /// Validates the request DTO
    /// </summary>
    public bool IsValid()
    {
        return EmployeeNumber > 0
            && !string.IsNullOrWhiteSpace(AbsenceType)
            && !string.IsNullOrWhiteSpace(Employer)
            && !string.IsNullOrWhiteSpace(StartDate)
            && !string.IsNullOrWhiteSpace(EndDate)
            && !string.IsNullOrWhiteSpace(AbsenceStatusCode)
            && !string.IsNullOrWhiteSpace(ApprovalStatusCode)
            && StartDateDuration >= 0 && StartDateDuration <= 1
            && EndDateDuration >= 0 && EndDateDuration <= 1;
    }
}
