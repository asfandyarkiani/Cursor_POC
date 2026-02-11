using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CAFMManagementSystem.ConfigModels;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CAFMManagementSystem.Helper
{
    public class KeyVaultReader
    {
        private readonly SecretClient _secretClient;
        private readonly KeyVaultConfigs _keyVaultConfigs;
        private readonly ILogger<KeyVaultReader> _logger;
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
            
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            
            _logger.Info($"Secret {secretName} fetched successfully");
            return secret.Value;
        }
        
        public async Task<Dictionary<string, string>> GetAuthSecretsAsync()
        {
            await _cacheLock.WaitAsync();
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                
                if (_keyVaultConfigs.Secrets == null)
                {
                    _logger.Warn("No secrets configured in KeyVaultConfigs");
                    return result;
                }
                
                foreach (KeyValuePair<string, string> secretMapping in _keyVaultConfigs.Secrets)
                {
                    string secretKey = secretMapping.Key;
                    string secretName = secretMapping.Value;
                    
                    if (!_secretCache.ContainsKey(secretKey))
                    {
                        _logger.Info($"Caching secret: {secretKey}");
                        _secretCache[secretKey] = await GetSecretAsync(secretName);
                    }
                    
                    result[secretKey] = _secretCache[secretKey];
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
