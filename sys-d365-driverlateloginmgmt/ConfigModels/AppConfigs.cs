using Core.Validators;

namespace AGI.SysD365DriverLateLoginMgmt.ConfigModels;

/// <summary>
/// Application configuration model for D365 Driver Late Login Management
/// </summary>
public class AppConfigs : IConfigValidator
{
    /// <summary>
    /// D365 configuration settings
    /// </summary>
    public D365Config D365Config { get; set; } = new D365Config();

    /// <summary>
    /// Azure Key Vault configuration settings
    /// </summary>
    public KeyVaultConfig KeyVaultConfig { get; set; } = new KeyVaultConfig();

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>Tuple with validation result and error message</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        // Validate D365 configuration
        (bool d365IsValid, string d365ErrorMessage) = D365Config.Validate();
        if (!d365IsValid)
        {
            return (false, $"D365 configuration validation failed: {d365ErrorMessage}");
        }

        // Validate Key Vault configuration if enabled
        if (KeyVaultConfig.UseKeyVault)
        {
            (bool kvIsValid, string kvErrorMessage) = KeyVaultConfig.Validate();
            if (!kvIsValid)
            {
                return (false, $"Key Vault configuration validation failed: {kvErrorMessage}");
            }
        }

        return (true, string.Empty);
    }
}

/// <summary>
/// D365 configuration settings
/// </summary>
public class D365Config : IConfigValidator
{
    /// <summary>
    /// Base URL for D365 API (e.g., https://agifd-dev.cloudax.uae.dynamics.com)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for late login API endpoint
    /// </summary>
    public string LateLoginResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD token endpoint URL
    /// </summary>
    public string TokenUrl { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 client ID for D365 authentication
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 client secret for D365 authentication
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Resource URL for token request (D365 base URL)
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 grant type (default: client_credentials)
    /// </summary>
    public string GrantType { get; set; } = "client_credentials";

    /// <summary>
    /// Token cache duration in minutes (default: 50 minutes, Azure AD tokens expire in 60 minutes)
    /// </summary>
    public int TokenCacheDurationMinutes { get; set; } = 50;

    /// <summary>
    /// Validates the D365 configuration
    /// </summary>
    /// <returns>Tuple with validation result and error message</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            return (false, "BaseUrl is required");
        }

        if (string.IsNullOrWhiteSpace(LateLoginResourcePath))
        {
            return (false, "LateLoginResourcePath is required");
        }

        if (string.IsNullOrWhiteSpace(TokenUrl))
        {
            return (false, "TokenUrl is required");
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            return (false, "ClientId is required");
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            return (false, "ClientSecret is required");
        }

        if (string.IsNullOrWhiteSpace(Resource))
        {
            return (false, "Resource is required");
        }

        if (TokenCacheDurationMinutes <= 0 || TokenCacheDurationMinutes > 60)
        {
            return (false, "TokenCacheDurationMinutes must be between 1 and 60 minutes");
        }

        return (true, string.Empty);
    }
}

/// <summary>
/// Azure Key Vault configuration settings
/// </summary>
public class KeyVaultConfig : IConfigValidator
{
    /// <summary>
    /// Azure Key Vault URL
    /// </summary>
    public string KeyVaultUrl { get; set; } = string.Empty;

    /// <summary>
    /// Flag to enable/disable Key Vault usage
    /// </summary>
    public bool UseKeyVault { get; set; } = false;

    /// <summary>
    /// Validates the Key Vault configuration
    /// </summary>
    /// <returns>Tuple with validation result and error message</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (UseKeyVault && string.IsNullOrWhiteSpace(KeyVaultUrl))
        {
            return (false, "KeyVaultUrl is required when UseKeyVault is true");
        }

        return (true, string.Empty);
    }
}
