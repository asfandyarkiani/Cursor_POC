using Core.DTOs;

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
    public void validate()
    {
        List<string> errors = new List<string>();

        // Validate D365 configuration
        try
        {
            D365Config.validate();
        }
        catch (InvalidOperationException ex)
        {
            errors.Add($"D365 configuration validation failed: {ex.Message}");
        }

        // Validate Key Vault configuration if enabled
        if (KeyVaultConfig.UseKeyVault)
        {
            try
            {
                KeyVaultConfig.validate();
            }
            catch (InvalidOperationException ex)
            {
                errors.Add($"Key Vault configuration validation failed: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
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
    public void validate()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            errors.Add("BaseUrl is required");
        }

        if (string.IsNullOrWhiteSpace(LateLoginResourcePath))
        {
            errors.Add("LateLoginResourcePath is required");
        }

        if (string.IsNullOrWhiteSpace(TokenUrl))
        {
            errors.Add("TokenUrl is required");
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            errors.Add("ClientId is required");
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            errors.Add("ClientSecret is required");
        }

        if (string.IsNullOrWhiteSpace(Resource))
        {
            errors.Add("Resource is required");
        }

        if (TokenCacheDurationMinutes <= 0 || TokenCacheDurationMinutes > 60)
        {
            errors.Add("TokenCacheDurationMinutes must be between 1 and 60 minutes");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"D365Config validation failed:\n{string.Join("\n", errors)}");
        }
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
    public void validate()
    {
        List<string> errors = new List<string>();

        if (UseKeyVault && string.IsNullOrWhiteSpace(KeyVaultUrl))
        {
            errors.Add("KeyVaultUrl is required when UseKeyVault is true");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"KeyVaultConfig validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
