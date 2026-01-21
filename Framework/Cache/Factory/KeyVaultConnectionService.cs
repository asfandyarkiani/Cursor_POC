using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Cache.ConfigModels;

namespace Cache.Factory
{
    internal class KeyVaultConnectionService
    {
        private static SecretClient _secretClient;

        public static void Initialize(KeyVaultConfigs keyVaultConfigs)
        {
            keyVaultConfigs.validate();
            _secretClient = new SecretClient(new Uri(keyVaultConfigs.Url), new DefaultAzureCredential());
        }

        public static KeyVaultResponse GetSecret(string secretName)
        {
            KeyVaultResponse response = new KeyVaultResponse();
            if (_secretClient == null)
            {
                throw new InvalidOperationException("Key Vault service is not initialized.");
            }

            KeyVaultSecret secret = _secretClient.GetSecret(secretName);

            if (secret != null && !string.IsNullOrWhiteSpace(secret.Value))
            {
                response = new KeyVaultResponse
                {
                    IsSuccess = true,
                    Secret = secret.Value
                };

                return response;
            }

            return response;
        }
    }
}
