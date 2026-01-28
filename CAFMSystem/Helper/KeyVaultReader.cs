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
    /// <summary>
    /// KeyVault reader for retrieving secrets from Azure KeyVault.
    /// Implements caching to minimize KeyVault calls.
    /// </summary>
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

        /// <summary>
        /// Retrieves a single secret from KeyVault.
        /// </summary>
        /// <param name="secretName">Secret name</param>
        /// <returns>Secret value</returns>
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
                _logger.Error(ex, $"Failed to retrieve secret: {secretName}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves multiple secrets from KeyVault with caching.
        /// </summary>
        /// <param name="secretNames">List of secret names</param>
        /// <returns>Dictionary of secret name to secret value</returns>
        public async Task<Dictionary<string, string>> GetSecretsAsync(List<string> secretNames)
        {
            await _cacheLock.WaitAsync();
            try
            {
                Dictionary<string, string> secretsResult = new Dictionary<string, string>();

                foreach (string secretName in secretNames)
                {
                    if (!_secretCache.ContainsKey(secretName))
                    {
                        _logger.Info($"Caching secret: {secretName}");
                        _secretCache[secretName] = await GetSecretAsync(secretName);
                    }

                    secretsResult[secretName] = _secretCache[secretName];
                }

                return secretsResult;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        /// <summary>
        /// Retrieves authentication secrets (username and password) from KeyVault.
        /// </summary>
        /// <returns>Dictionary with FSI_Username and FSI_Password</returns>
        public async Task<Dictionary<string, string>> GetAuthSecretsAsync()
        {
            if (_keyVaultConfigs.Secrets == null)
                throw new InvalidOperationException("KeyVault Secrets configuration is missing.");

            List<string> secretNames = new List<string>
            {
                _keyVaultConfigs.Secrets.GetValueOrDefault("FSI_Username", "FSI-Username"),
                _keyVaultConfigs.Secrets.GetValueOrDefault("FSI_Password", "FSI-Password")
            };

            Dictionary<string, string> secrets = await GetSecretsAsync(secretNames);

            return new Dictionary<string, string>
            {
                { "Username", secrets[secretNames[0]] },
                { "Password", secrets[secretNames[1]] }
            };
        }
    }
}
