using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CAFMSystem.ConfigModels;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CAFMSystem.Helper
{
    public class KeyVaultReader
    {
        private readonly SecretClient _secretClient;
        private readonly KeyVaultConfigs _kvConfigs;
        private readonly ILogger<KeyVaultReader> _logger;
        private static readonly Dictionary<string, string> _secretCache = new Dictionary<string, string>();
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
            _logger.Info($"Fetching secret: {secretName} from Key Vault");
            
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            
            _logger.Info($"Secret {secretName} retrieved successfully");
            
            return secret.Value;
        }

        public async Task<Dictionary<string, string>> GetAuthSecretsAsync()
        {
            await _cacheLock.WaitAsync();
            
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                
                if (_kvConfigs.Secrets == null)
                {
                    _logger.Warn("No secrets configured in KeyVaultConfigs");
                    return result;
                }
                
                foreach (KeyValuePair<string, string> secretConfig in _kvConfigs.Secrets)
                {
                    string secretKey = secretConfig.Key;
                    string secretName = secretConfig.Value;
                    
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
