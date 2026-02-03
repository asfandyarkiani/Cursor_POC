using Core.SystemLayer.DTOs;

namespace AGI.SysD365DriverLateLoginMgmt.DTO.AtomicHandlerDTOs;

/// <summary>
/// Request DTO for SubmitLateLoginAtomicHandler
/// Used internally to pass data to Atomic Handler
/// </summary>
public class SubmitLateLoginHandlerReqDTO : IDownStreamRequestDTO
{
    /// <summary>
    /// Driver ID (required)
    /// </summary>
    public string DriverId { get; set; } = string.Empty;

    /// <summary>
    /// Late login date and time (optional)
    /// </summary>
    public string? RequestDateTime { get; set; }

    /// <summary>
    /// Company code (optional)
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
    /// OAuth2 Bearer token for D365 authentication
    /// </summary>
    public string AuthorizationToken { get; set; } = string.Empty;

    /// <summary>
    /// Validates the downstream request parameters
    /// </summary>
    public void ValidateDownStreamRequestParameters()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(DriverId))
        {
            errors.Add("DriverId is required");
        }

        if (string.IsNullOrWhiteSpace(AuthorizationToken))
        {
            errors.Add("AuthorizationToken is required");
        }

        // RequestDateTime and CompanyCode are optional (match D365 contract)

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: "SubmitLateLoginHandlerReqDTO.cs / ValidateDownStreamRequestParameters"
            );
        }
    }
}
