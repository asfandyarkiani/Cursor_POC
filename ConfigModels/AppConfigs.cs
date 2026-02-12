namespace FacilitiesMgmtSystem.ConfigModels;

/// <summary>
/// Application configuration model. Populated from appsettings.{environment}.json.
/// </summary>
public class AppConfigs
{
    /// <summary>
    /// MRI (CAFM) service configuration.
    /// </summary>
    public MRIConfig MRI { get; set; } = new();

    /// <summary>
    /// Application-specific variables and settings.
    /// </summary>
    public AppVariables AppVariables { get; set; } = new();

    /// <summary>
    /// HTTP client configuration.
    /// </summary>
    public HttpClientConfig HttpClient { get; set; } = new();
}

/// <summary>
/// MRI (CAFM) service configuration.
/// </summary>
public class MRIConfig
{
    /// <summary>
    /// Base URL for MRI SOAP services.
    /// TODO: Populate from secure configuration source.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Login endpoint path.
    /// </summary>
    public string LoginEndpoint { get; set; } = "/Login";

    /// <summary>
    /// Logout endpoint path.
    /// </summary>
    public string LogoutEndpoint { get; set; } = "/Logout";

    /// <summary>
    /// Create Work Order endpoint path.
    /// </summary>
    public string CreateWorkOrderEndpoint { get; set; } = "/WorkOrder/Create";

    /// <summary>
    /// Get Breakdown Task endpoint path.
    /// </summary>
    public string GetBreakdownTaskEndpoint { get; set; } = "/Task/GetBreakdown";

    /// <summary>
    /// Get Location endpoint path.
    /// </summary>
    public string GetLocationEndpoint { get; set; } = "/Location/Get";

    /// <summary>
    /// Get Instruction Sets endpoint path.
    /// </summary>
    public string GetInstructionSetsEndpoint { get; set; } = "/InstructionSets/Get";

    /// <summary>
    /// MRI service username.
    /// TODO: Store in secure secrets management (Key Vault).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// MRI service password.
    /// TODO: Store in secure secrets management (Key Vault).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// MRI contract ID.
    /// TODO: Store in secure secrets management (Key Vault).
    /// </summary>
    public string ContractId { get; set; } = string.Empty;

    /// <summary>
    /// SOAP action namespace for MRI operations.
    /// </summary>
    public string SoapActionNamespace { get; set; } = "http://tempuri.org/";

    /// <summary>
    /// MRI service namespace for XML elements.
    /// </summary>
    public string ServiceNamespace { get; set; } = "http://tempuri.org/";
}

/// <summary>
/// Application-specific variables and settings.
/// </summary>
public class AppVariables
{
    /// <summary>
    /// Application name for logging and identification.
    /// </summary>
    public string ApplicationName { get; set; } = "FacilitiesMgmtSystem";

    /// <summary>
    /// Current environment name.
    /// </summary>
    public string Environment { get; set; } = "dev";

    /// <summary>
    /// Enable detailed logging for debugging.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}

/// <summary>
/// HTTP client configuration for resilience and timeouts.
/// </summary>
public class HttpClientConfig
{
    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for transient failures.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Base delay between retries in seconds (exponential backoff).
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Circuit breaker failure threshold.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Circuit breaker sampling duration in seconds.
    /// </summary>
    public int CircuitBreakerSamplingDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Circuit breaker break duration in seconds.
    /// </summary>
    public int CircuitBreakerBreakDurationSeconds { get; set; } = 60;
}
