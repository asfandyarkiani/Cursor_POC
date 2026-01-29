using System.ComponentModel.DataAnnotations;

namespace SystemLayer.Infrastructure.Configuration;

/// <summary>
/// Configuration options for CAFM integration
/// </summary>
public class CafmOptions
{
    public const string SectionName = "Cafm";
    
    /// <summary>
    /// CAFM service base URL
    /// </summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// CAFM service endpoint path
    /// </summary>
    [Required]
    public string ServicePath { get; set; } = "/services/CafmService.asmx";
    
    /// <summary>
    /// CAFM username for authentication
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// CAFM password for authentication (should come from Key Vault)
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// CAFM database name
    /// </summary>
    [Required]
    public string Database { get; set; } = string.Empty;
    
    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum retry attempts for transient failures
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Base delay for exponential backoff in milliseconds
    /// </summary>
    [Range(100, 10000)]
    public int BaseDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Circuit breaker failure threshold
    /// </summary>
    [Range(1, 100)]
    public int CircuitBreakerThreshold { get; set; } = 5;
    
    /// <summary>
    /// Circuit breaker duration in seconds
    /// </summary>
    [Range(1, 300)]
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
    
    /// <summary>
    /// SOAP namespace for CAFM services
    /// </summary>
    public string SoapNamespace { get; set; } = "http://cafm.mri.com/services";
    
    /// <summary>
    /// Enable detailed logging (be careful with sensitive data)
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Session token cache duration in minutes
    /// </summary>
    [Range(1, 60)]
    public int SessionCacheDurationMinutes { get; set; } = 15;
}

/// <summary>
/// Azure Key Vault configuration options
/// </summary>
public class KeyVaultOptions
{
    public const string SectionName = "KeyVault";
    
    /// <summary>
    /// Key Vault URL
    /// </summary>
    [Url]
    public string? VaultUrl { get; set; }
    
    /// <summary>
    /// Whether to use Key Vault for secrets
    /// </summary>
    public bool Enabled { get; set; } = false;
    
    /// <summary>
    /// Managed identity client ID (optional)
    /// </summary>
    public string? ClientId { get; set; }
}

/// <summary>
/// Resilience policy configuration
/// </summary>
public class ResilienceOptions
{
    public const string SectionName = "Resilience";
    
    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public RetryPolicyOptions Retry { get; set; } = new();
    
    /// <summary>
    /// Circuit breaker policy configuration
    /// </summary>
    public CircuitBreakerPolicyOptions CircuitBreaker { get; set; } = new();
    
    /// <summary>
    /// Timeout policy configuration
    /// </summary>
    public TimeoutPolicyOptions Timeout { get; set; } = new();
}

public class RetryPolicyOptions
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    [Range(0, 10)]
    public int MaxAttempts { get; set; } = 3;
    
    /// <summary>
    /// Base delay for exponential backoff in milliseconds
    /// </summary>
    [Range(100, 10000)]
    public int BaseDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Maximum delay for exponential backoff in milliseconds
    /// </summary>
    [Range(1000, 60000)]
    public int MaxDelayMs { get; set; } = 10000;
}

public class CircuitBreakerPolicyOptions
{
    /// <summary>
    /// Number of consecutive failures before opening circuit
    /// </summary>
    [Range(1, 100)]
    public int FailureThreshold { get; set; } = 5;
    
    /// <summary>
    /// Duration to keep circuit open in seconds
    /// </summary>
    [Range(1, 300)]
    public int DurationSeconds { get; set; } = 30;
    
    /// <summary>
    /// Minimum throughput before circuit breaker activates
    /// </summary>
    [Range(1, 100)]
    public int MinimumThroughput { get; set; } = 10;
}

public class TimeoutPolicyOptions
{
    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
}