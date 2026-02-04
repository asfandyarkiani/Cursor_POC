using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AGI.SysD365DriverLateLoginMgmt.ConfigModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AGI.SysD365DriverLateLoginMgmt.Helper;

/// <summary>
/// Helper class for reading secrets from Azure Key Vault
/// </summary>
public class KeyVaultReader
{
    private readonly ILogger<KeyVaultReader> _logger;
    private readonly AppConfigs _appConfigs;
    private SecretClient? _secretClient;

    public KeyVaultReader(ILogger<KeyVaultReader> logger, IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _appConfigs = appConfigs.Value;

        if (_appConfigs.KeyVaultConfig.UseKeyVault && !string.IsNullOrWhiteSpace(_appConfigs.KeyVaultConfig.KeyVaultUrl))
        {
            Uri keyVaultUri = new Uri(_appConfigs.KeyVaultConfig.KeyVaultUrl);
            _secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());
            _logger.LogInformation("KeyVaultReader initialized with Key Vault URL: {KeyVaultUrl}", _appConfigs.KeyVaultConfig.KeyVaultUrl);
        }
        else
        {
            _logger.LogWarning("Key Vault is not configured or disabled. Secrets will be read from configuration.");
        }
    }

    /// <summary>
    /// Reads a secret from Azure Key Vault
    /// </summary>
    /// <param name="secretName">Name of the secret to read</param>
    /// <returns>Secret value</returns>
    public async Task<string> GetSecretAsync(string secretName)
    {
        if (_secretClient == null)
        {
            _logger.LogWarning("Key Vault client is not initialized. Returning empty string for secret: {SecretName}", secretName);
            return string.Empty;
        }

        try
        {
            _logger.LogInformation("Reading secret from Key Vault: {SecretName}", secretName);
            KeyVaultSecret keyVaultSecret = await _secretClient.GetSecretAsync(secretName);
            _logger.LogInformation("Successfully retrieved secret from Key Vault: {SecretName}", secretName);
            return keyVaultSecret.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read secret from Key Vault: {SecretName}", secretName);
            throw;
        }
    }

    /// <summary>
    /// Reads a secret from Azure Key Vault or falls back to configuration
    /// </summary>
    /// <param name="secretName">Name of the secret to read from Key Vault</param>
    /// <param name="fallbackValue">Fallback value from configuration</param>
    /// <returns>Secret value from Key Vault or fallback value</returns>
    public async Task<string> GetSecretOrFallbackAsync(string secretName, string fallbackValue)
    {
        if (!_appConfigs.KeyVaultConfig.UseKeyVault || _secretClient == null)
        {
            _logger.LogInformation("Using fallback value for secret: {SecretName}", secretName);
            return fallbackValue;
        }

        try
        {
            return await GetSecretAsync(secretName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read secret from Key Vault: {SecretName}. Using fallback value.", secretName);
            return fallbackValue;
        }
    }
}
