using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CAFMSystem.ConfigModels;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CAFMSystem.Helper
{
    public class KeyVaultReader
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<KeyVaultReader> _logger;
        private readonly KeyVaultConfigs _keyVaultConfigs;
        
        private static readonly Dictionary<string, string> _secretCache = new Dictionary<string, string>();
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        public KeyVaultReader(IOptions<KeyVaultConfigs> options, ILogger<KeyVaultReader> logger)
        {
            _logger = logger;
            _keyVaultConfigs = options.Value;
            _keyVaultConfigs.validate();
            
            Uri keyVaultUrl = new Uri(_keyVaultConfigs.Url);
            _secretClient = new SecretClient(keyVaultUrl, new DefaultAzureCredential());
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            _logger.Info($"Fetching secret: {secretName} from Key Vault");
            
            try
            {
                KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
                
                if (secret != null && !string.IsNullOrEmpty(secret.Value))
                {
                    _logger.Info($"Successfully fetched secret: {secretName}");
                    return secret.Value;
                }
                
                _logger.Warn($"Secret {secretName} is null or empty");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to fetch secret: {secretName}");
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetSecretsAsync(List<string> secretNames)
        {
            await _cacheLock.WaitAsync();
            
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                
                foreach (string secretName in secretNames)
                {
                    if (!_secretCache.ContainsKey(secretName))
                    {
                        _logger.Info($"Caching secret: {secretName}");
                        _secretCache[secretName] = await GetSecretAsync(secretName);
                    }
                    
                    result[secretName] = _secretCache[secretName];
                }
                
                return result;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public async Task<Dictionary<string, string?>> GetAuthSecretsAsync()
        {
            Dictionary<string, string?> secrets = new Dictionary<string, string?>();
            
            if (_keyVaultConfigs.Secrets != null)
            {
                foreach (KeyValuePair<string, string> kvp in _keyVaultConfigs.Secrets)
                {
                    try
                    {
                        secrets[kvp.Key] = await GetSecretAsync(kvp.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Failed to get secret for key: {kvp.Key}");
                        secrets[kvp.Key] = null;
                    }
                }
            }
            
            return secrets;
        }
    }
}
