using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sys_cafm_mgmt.ConfigModels;

namespace sys_cafm_mgmt.Helper
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
            
            _logger.Info($"Successfully fetched secret: {secretName}");
            return secret.Value;
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

        public async Task<Dictionary<string, string>> GetAuthSecretsAsync()
        {
            List<string> secretNames = new List<string>();
            
            if (_keyVaultConfigs.Secrets != null)
            {
                if (_keyVaultConfigs.Secrets.ContainsKey("FsiUsername"))
                    secretNames.Add(_keyVaultConfigs.Secrets["FsiUsername"]);
                
                if (_keyVaultConfigs.Secrets.ContainsKey("FsiPassword"))
                    secretNames.Add(_keyVaultConfigs.Secrets["FsiPassword"]);
            }
            
            return await GetSecretsAsync(secretNames);
        }
    }
}
