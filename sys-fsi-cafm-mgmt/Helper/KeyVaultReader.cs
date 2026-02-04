using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Extensions;
using FsiCafmSystem.ConfigModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FsiCafmSystem.Helper
{
    public class KeyVaultReader
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<KeyVaultReader> _logger;
        private static readonly Dictionary<string, string> _secretCache = new();
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        
        public KeyVaultReader(IOptions<KeyVaultConfigs> options, ILogger<KeyVaultReader> logger)
        {
            _logger = logger;
            KeyVaultConfigs kvConfigs = options.Value;
            kvConfigs.validate();
            
            Uri keyVaultUrl = new Uri(kvConfigs.Url);
            _secretClient = new SecretClient(keyVaultUrl, new DefaultAzureCredential());
        }
        
        public async Task<string> GetSecretAsync(string secretName)
        {
            _logger.Info($"Fetching secret: {secretName} from Key Vault");
            
            try
            {
                KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
                _logger.Info($"Secret retrieved successfully: {secretName}");
                return secret.Value;
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
    }
}
