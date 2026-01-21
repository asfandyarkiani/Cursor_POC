using Core.DTOs;
using Core.Exceptions;

namespace Cache.ConfigModels
{
    public class KeyVaultConfigs : IConfigValidator
    {
        public string Url { get; set; } = string.Empty;

        public void validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Url))
            {
                errors.Add("Vault URL is required.");
            }

            if (errors.Count > 0)
            {
                throw new RequestValidationFailureException(
                  errorDetails: errors,
                  stepName: "KeyVaultConfigs.cs / Executing validate"
                );
            }
        }
    }
}
