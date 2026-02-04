using System.Text.Json.Serialization;

namespace AGI.SysD365DriverLateLoginMgmt.DTO.DownstreamDTOs;

/// <summary>
/// Response DTO from Azure AD OAuth2 token endpoint
/// Represents the actual response structure from Azure AD
/// </summary>
public class AuthenticationResponseDTO
{
    /// <summary>
    /// Token type (e.g., "Bearer")
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; } = string.Empty;

    /// <summary>
    /// Extended expiration time in seconds
    /// </summary>
    [JsonPropertyName("ext_expires_in")]
    public string ExtExpiresIn { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration timestamp (Unix timestamp)
    /// </summary>
    [JsonPropertyName("expires_on")]
    public string ExpiresOn { get; set; } = string.Empty;

    /// <summary>
    /// Token not valid before timestamp (Unix timestamp)
    /// </summary>
    [JsonPropertyName("not_before")]
    public string NotBefore { get; set; } = string.Empty;

    /// <summary>
    /// Resource URL
    /// </summary>
    [JsonPropertyName("resource")]
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 access token (CRITICAL)
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}
