using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHCMSystemLayer.ConfigModels;

namespace OracleFusionHCMSystemLayer.Helper
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
            _logger.Info($"Fetching secret: {secretName} from KeyVault");

            try
            {
                KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
                _logger.Info($"Successfully retrieved secret: {secretName}");
                return secret.Value;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to retrieve secret: {secretName} from KeyVault");
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

        public async Task<Dictionary<string, string>> GetOracleFusionCredentialsAsync()
        {
            List<string> secretNames = new List<string>
            {
                KeyVaultConfigs.ORACLE_FUSION_USERNAME_KEY,
                KeyVaultConfigs.ORACLE_FUSION_PASSWORD_KEY
            };

            return await GetSecretsAsync(secretNames);
        }
    }
}
