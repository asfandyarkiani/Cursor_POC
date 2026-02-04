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
    /// Late login date and time (optional but recommended)
    /// Format: ISO 8601 (e.g., "2024-01-15T08:30:00Z")
    /// Note: While optional in D365 contract, the Boomi process always provides this value
    /// </summary>
    public string? RequestDateTime { get; set; }

    /// <summary>
    /// Company code (optional but recommended)
    /// Note: While optional in D365 contract, the Boomi process always provides this value
    /// </summary>
    public string? CompanyCode { get; set; }

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
    public void ValidateAPIRequestParameters()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(DriverId))
        {
            errors.Add("DriverId is required and cannot be empty");
        }

        // RequestDateTime is optional, but if provided, must be valid
        if (!string.IsNullOrWhiteSpace(RequestDateTime) && !DateTime.TryParse(RequestDateTime, out DateTime _))
        {
            errors.Add("RequestDateTime must be a valid date/time in ISO 8601 format");
        }

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: "SubmitDriverLateLoginReqDTO.cs / ValidateAPIRequestParameters"
            );
        }
    }
}
