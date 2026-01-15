namespace FacilitiesMgmtSystem.ConfigModels;

/// <summary>
/// Application configuration model.
/// All configuration values should be accessed through this class via IOptions<AppConfigs>.
/// </summary>
public class AppConfigs
{
    /// <summary>
    /// MRI SOAP API base URL.
    /// </summary>
    public string MriBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// MRI Login SOAP action URL.
    /// </summary>
    public string MriLoginSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// MRI Logout SOAP action URL.
    /// </summary>
    public string MriLogoutSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// MRI Username - should be retrieved from secure source (Key Vault, etc.).
    /// </summary>
    public string MriUsername { get; set; } = string.Empty;

    /// <summary>
    /// MRI Password - should be retrieved from secure source (Key Vault, etc.).
    /// </summary>
    public string MriPassword { get; set; } = string.Empty;

    /// <summary>
    /// MRI Contract ID for CAFM operations.
    /// </summary>
    public string MriContractId { get; set; } = string.Empty;

    /// <summary>
    /// HTTP client timeout in seconds.
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for transient failures.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Application environment name.
    /// </summary>
    public string Environment { get; set; } = "dev";
}
