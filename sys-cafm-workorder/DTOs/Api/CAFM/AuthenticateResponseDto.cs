namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Response DTO for CAFM Authentication API.
/// </summary>
public class AuthenticateResponseDto
{
    /// <summary>
    /// The session ID returned from CAFM after successful authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether authentication was successful.
    /// </summary>
    public bool IsAuthenticated { get; set; }
}
