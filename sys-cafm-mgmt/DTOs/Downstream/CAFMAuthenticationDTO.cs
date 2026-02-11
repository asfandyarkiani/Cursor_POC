namespace AGI.SystemLayer.CAFM.DTOs.Downstream;

/// <summary>
/// DTO for CAFM authentication request
/// </summary>
public class CAFMAuthenticationRequestDTO
{
    public string LoginName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for CAFM authentication response
/// </summary>
public class CAFMAuthenticationResponseDTO
{
    public string? SessionId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for CAFM logout request
/// </summary>
public class CAFMLogoutRequestDTO
{
    public string SessionId { get; set; } = string.Empty;
}
