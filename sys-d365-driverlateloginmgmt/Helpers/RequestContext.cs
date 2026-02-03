namespace AGI.SysD365DriverLateLoginMgmt.Helper;

/// <summary>
/// Request context for storing authentication information
/// Used by CustomAuthenticationMiddleware to store token information
/// </summary>
public class RequestContext
{
    /// <summary>
    /// OAuth2 Bearer token for D365 authentication
    /// </summary>
    public string? AuthorizationToken { get; set; }

    /// <summary>
    /// Token expiration timestamp (Unix timestamp)
    /// </summary>
    public long? TokenExpiresOn { get; set; }

    /// <summary>
    /// Flag indicating if token is valid
    /// </summary>
    public bool IsTokenValid { get; set; }

    /// <summary>
    /// Checks if the token is expired
    /// </summary>
    /// <returns>True if token is expired, false otherwise</returns>
    public bool IsTokenExpired()
    {
        if (!TokenExpiresOn.HasValue)
        {
            return true;
        }

        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        // Add 5 minute buffer to refresh token before it actually expires
        long bufferSeconds = 300; // 5 minutes
        return currentTimestamp >= (TokenExpiresOn.Value - bufferSeconds);
    }

    /// <summary>
    /// Clears the authentication context
    /// </summary>
    public void Clear()
    {
        AuthorizationToken = null;
        TokenExpiresOn = null;
        IsTokenValid = false;
    }
}
