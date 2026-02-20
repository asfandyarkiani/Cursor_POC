using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHcmSystemLayer.ConfigModels;

namespace OracleFusionHcmSystemLayer.Helper
{
    public class KeyVaultReader
    {
        private readonly SecretClient _secretClient;
        private readonly KeyVaultConfigs _kvConfigs;
        private readonly ILogger<KeyVaultReader> _logger;
        private static readonly Dictionary<string, string> _secretCache = new();
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        public KeyVaultReader(IOptions<KeyVaultConfigs> options, ILogger<KeyVaultReader> logger)
        {
            _logger = logger;
            _kvConfigs = options.Value;
            _kvConfigs.validate();
            _secretClient = new SecretClient(new Uri(_kvConfigs.Url), new DefaultAzureCredential());
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            _logger.Info($"Fetching secret: {secretName} from Key Vault started.");
            
            string cleanedSecretName = secretName.Replace(" ", "").ToLowerInvariant();
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(cleanedSecretName);
            
            if (secret != null && secret.Value != null)
            {
                _logger.Info($"Fetching secret: {secretName} from Key Vault completed.");
                return secret.Value;
            }
            
            _logger.Warn($"Fetching secret: {secretName} from Key Vault couldn't succeed.");
            return string.Empty;
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
