using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Request DTO for CAFM Logout API.
/// </summary>
public class LogoutRequestDto : IRequestSysDTO
{
    /// <summary>
    /// The session ID to terminate.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    public void ValidateAPIRequestParameters()
    {
        if (string.IsNullOrWhiteSpace(SessionId))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: new List<string> { "SessionId is required for logout." },
                stepName: nameof(LogoutRequestDto));
        }
    }
}
