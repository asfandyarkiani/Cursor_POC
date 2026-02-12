using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Request DTO for CAFM Authentication API.
/// </summary>
public class AuthenticateRequestDto : IRequestSysDTO
{
    /// <summary>
    /// FSI Login username.
    /// If not provided, will use configured default from AppConfigs.
    /// </summary>
    public string? LoginName { get; set; }

    /// <summary>
    /// FSI Login password.
    /// If not provided, will use configured default from AppConfigs.
    /// </summary>
    public string? Password { get; set; }

    public void ValidateAPIRequestParameters()
    {
        // LoginName and Password can be optional if using config defaults
        // Validation is performed at the handler level
    }
}
