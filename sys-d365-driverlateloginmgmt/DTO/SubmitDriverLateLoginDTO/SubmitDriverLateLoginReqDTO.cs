using Core.SystemLayer.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AGI.SysD365DriverLateLoginMgmt.DTO.SubmitDriverLateLoginDTO;

/// <summary>
/// Request DTO for submitting driver late login request to D365
/// Received from Process Layer
/// </summary>
public class SubmitDriverLateLoginReqDTO : IRequestSysDTO
{
    /// <summary>
    /// Driver ID (required)
    /// </summary>
    [Required(ErrorMessage = "DriverId is required")]
    public string DriverId { get; set; } = string.Empty;

    /// <summary>
    /// Late login date and time (required)
    /// Format: ISO 8601 (e.g., "2024-01-15T08:30:00Z")
    /// </summary>
    [Required(ErrorMessage = "RequestDateTime is required")]
    public string RequestDateTime { get; set; } = string.Empty;

    /// <summary>
    /// Company code (required)
    /// </summary>
    [Required(ErrorMessage = "CompanyCode is required")]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// Reason code for late login (optional)
    /// </summary>
    public string? ReasonCode { get; set; }

    /// <summary>
    /// Remarks/comments for late login (optional)
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Request number (optional)
    /// </summary>
    public string? RequestNo { get; set; }

    /// <summary>
    /// Validates the request DTO
    /// </summary>
    /// <returns>Tuple with validation result and error message</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(DriverId))
        {
            return (false, "DriverId is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(RequestDateTime))
        {
            return (false, "RequestDateTime is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(CompanyCode))
        {
            return (false, "CompanyCode is required and cannot be empty");
        }

        // Validate RequestDateTime format (ISO 8601)
        if (!DateTime.TryParse(RequestDateTime, out DateTime _))
        {
            return (false, "RequestDateTime must be a valid date/time in ISO 8601 format");
        }

        return (true, string.Empty);
    }
}
