using AlGhurair.Core.SystemLayer.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AlGhurair.SystemLayer.OracleFusionHCM.DTO.CreateLeaveDTO;

/// <summary>
/// Request DTO for Create Leave API
/// Represents leave creation request from D365 or Process Layer
/// </summary>
public class CreateLeaveReqDTO : IRequestSysDTO
{
    /// <summary>
    /// Employee number from D365
    /// </summary>
    [Required(ErrorMessage = "Employee number is required")]
    public int EmployeeNumber { get; set; }

    /// <summary>
    /// Type of absence/leave (e.g., "Sick Leave", "Annual Leave")
    /// </summary>
    [Required(ErrorMessage = "Absence type is required")]
    public string AbsenceType { get; set; } = string.Empty;

    /// <summary>
    /// Employer name
    /// </summary>
    [Required(ErrorMessage = "Employer is required")]
    public string Employer { get; set; } = string.Empty;

    /// <summary>
    /// Leave start date (YYYY-MM-DD format)
    /// </summary>
    [Required(ErrorMessage = "Start date is required")]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// Leave end date (YYYY-MM-DD format)
    /// </summary>
    [Required(ErrorMessage = "End date is required")]
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Absence status code (e.g., "SUBMITTED", "APPROVED")
    /// </summary>
    [Required(ErrorMessage = "Absence status code is required")]
    public string AbsenceStatusCode { get; set; } = string.Empty;

    /// <summary>
    /// Approval status code (e.g., "APPROVED", "PENDING")
    /// </summary>
    [Required(ErrorMessage = "Approval status code is required")]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    /// <summary>
    /// Duration on start date (typically 1 for full day, 0.5 for half day)
    /// </summary>
    [Required(ErrorMessage = "Start date duration is required")]
    [Range(0.1, 1.0, ErrorMessage = "Start date duration must be between 0.1 and 1.0")]
    public decimal StartDateDuration { get; set; }

    /// <summary>
    /// Duration on end date (typically 1 for full day, 0.5 for half day)
    /// </summary>
    [Required(ErrorMessage = "End date duration is required")]
    [Range(0.1, 1.0, ErrorMessage = "End date duration must be between 0.1 and 1.0")]
    public decimal EndDateDuration { get; set; }

    /// <summary>
    /// Validates the request DTO
    /// </summary>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (EmployeeNumber <= 0)
        {
            return (false, "Employee number must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(AbsenceType))
        {
            return (false, "Absence type cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(Employer))
        {
            return (false, "Employer cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(StartDate))
        {
            return (false, "Start date cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(EndDate))
        {
            return (false, "End date cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(AbsenceStatusCode))
        {
            return (false, "Absence status code cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(ApprovalStatusCode))
        {
            return (false, "Approval status code cannot be empty");
        }

        if (StartDateDuration < 0.1m || StartDateDuration > 1.0m)
        {
            return (false, "Start date duration must be between 0.1 and 1.0");
        }

        if (EndDateDuration < 0.1m || EndDateDuration > 1.0m)
        {
            return (false, "End date duration must be between 0.1 and 1.0");
        }

        // Validate date format (YYYY-MM-DD)
        if (!DateTime.TryParse(StartDate, out DateTime parsedStartDate))
        {
            return (false, "Invalid start date format. Expected YYYY-MM-DD");
        }

        if (!DateTime.TryParse(EndDate, out DateTime parsedEndDate))
        {
            return (false, "Invalid end date format. Expected YYYY-MM-DD");
        }

        if (parsedEndDate < parsedStartDate)
        {
            return (false, "End date cannot be before start date");
        }

        return (true, string.Empty);
    }
}
