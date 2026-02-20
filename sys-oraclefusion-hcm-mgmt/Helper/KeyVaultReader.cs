using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHcmMgmt.ConfigModels;

namespace OracleFusionHcmMgmt.Helper
{
    /// <summary>
    /// Helper class for reading secrets from Azure KeyVault.
    /// Supports caching for performance optimization.
    /// </summary>
    public class KeyVaultReader
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<KeyVaultReader> _logger;
        private static readonly Dictionary<string, string> _secretCache = new Dictionary<string, string>();
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        
        public KeyVaultReader(IOptions<KeyVaultConfigs> options, ILogger<KeyVaultReader> logger)
        {
            _logger = logger;
            KeyVaultConfigs config = options.Value;
            config.validate();
            
            Uri keyVaultUrl = new Uri(config.Url);
            _secretClient = new SecretClient(keyVaultUrl, new DefaultAzureCredential());
        }
        
        /// <summary>
        /// Retrieves a single secret from KeyVault.
        /// </summary>
        public async Task<string> GetSecretAsync(string secretName)
        {
            _logger.Info($"Fetching secret: {secretName} from KeyVault");
            
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            
            _logger.Info($"Successfully fetched secret: {secretName}");
            
            return secret.Value;
        }
        
        /// <summary>
        /// Retrieves multiple secrets from KeyVault with caching.
        /// </summary>
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
    }
}
