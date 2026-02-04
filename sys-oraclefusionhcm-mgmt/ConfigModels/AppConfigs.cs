namespace AlGhurair.SystemLayer.OracleFusionHCM.ConfigModels;

/// <summary>
/// Application configuration for Oracle Fusion HCM System Layer
/// </summary>
public class AppConfigs
{
    /// <summary>
    /// Oracle Fusion HCM API configuration
    /// </summary>
    public OracleFusionHCMConfig OracleFusionHCM { get; set; } = new();

    /// <summary>
    /// HTTP client configuration
    /// </summary>
    public HttpClientConfig HttpClient { get; set; } = new();
}

/// <summary>
/// Oracle Fusion HCM API configuration
/// </summary>
public class OracleFusionHCMConfig
{
    /// <summary>
    /// Base URL for Oracle Fusion HCM API
    /// Example: https://iaaxey-dev3.fa.ocs.oraclecloud.com:443
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for absences endpoint
    /// Example: hcmRestApi/resources/11.13.18.05/absences
    /// </summary>
    public string AbsencesResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Username for Basic Authentication
    /// Example: INTEGRATION.USER@al-ghurair.com
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for Basic Authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// HTTP client configuration
/// </summary>
public class HttpClientConfig
{
    /// <summary>
    /// Maximum retry attempts for failed requests
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry delay in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Circuit breaker failure threshold
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Circuit breaker duration in seconds
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}
