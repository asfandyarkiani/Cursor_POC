using Core.DTOs;
using sys_cafm_workorder.DTOs.Api.CAFM;

namespace sys_cafm_workorder.Abstractions;

/// <summary>
/// Interface for CAFM Authentication operations.
/// Handles login and logout from the CAFM (FSI) system.
/// </summary>
public interface ICAFMAuthenticationService
{
    /// <summary>
    /// Authenticates with CAFM and returns a session ID.
    /// </summary>
    /// <param name="request">Authentication request containing credentials (optional if using config defaults).</param>
    /// <returns>Response containing the session ID if authentication succeeds.</returns>
    Task<BaseResponseDTO> AuthenticateAsync(AuthenticateRequestDto request);

    /// <summary>
    /// Logs out from CAFM, terminating the specified session.
    /// </summary>
    /// <param name="request">Logout request containing the session ID to terminate.</param>
    /// <returns>Response indicating logout success/failure.</returns>
    Task<BaseResponseDTO> LogoutAsync(LogoutRequestDto request);
}
