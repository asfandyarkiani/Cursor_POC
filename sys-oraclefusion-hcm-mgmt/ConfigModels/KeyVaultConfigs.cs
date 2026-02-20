using Core.DTOs;

namespace OracleFusionHcmMgmt.ConfigModels
{
    /// <summary>
    /// Azure KeyVault configuration for secure secret management.
    /// Binds to "KeyVault" section in appsettings.json.
    /// </summary>
    public class KeyVaultConfigs : IConfigValidator
    {
        public static string SectionName = "KeyVault";
        
        public string Url { get; set; } = string.Empty;
        
        public Dictionary<string, string>? Secrets { get; set; }
        
        public void validate()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(Url))
            {
                errors.Add("KeyVault URL is required.");
            }
            else if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
            {
                errors.Add("KeyVault URL must be a valid URL.");
            }
            
            if (errors.Any())
            {
                throw new InvalidOperationException($"KeyVaultConfigs validation failed:\n{string.Join("\n", errors)}");
            }
        }
    }
}
