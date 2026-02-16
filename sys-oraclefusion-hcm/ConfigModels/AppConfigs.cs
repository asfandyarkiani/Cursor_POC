using Core.DTOs;

namespace sys_oraclefusion_hcm.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";

        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        
        // Oracle Fusion HCM API Configuration
        public string OracleFusionBaseUrl { get; set; } = string.Empty;
        public string OracleFusionResourcePath { get; set; } = string.Empty;
        public string OracleFusionUsername { get; set; } = string.Empty;
        public string OracleFusionPassword { get; set; } = string.Empty;
        
        // HTTP Client Configuration
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;

        public void validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(OracleFusionBaseUrl))
                errors.Add("OracleFusionBaseUrl is required");
            else if (!Uri.TryCreate(OracleFusionBaseUrl, UriKind.Absolute, out _))
                errors.Add("OracleFusionBaseUrl must be a valid URL");

            if (string.IsNullOrWhiteSpace(OracleFusionResourcePath))
                errors.Add("OracleFusionResourcePath is required");

            if (string.IsNullOrWhiteSpace(OracleFusionUsername))
                errors.Add("OracleFusionUsername is required");

            if (string.IsNullOrWhiteSpace(OracleFusionPassword))
                errors.Add("OracleFusionPassword is required");

            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                errors.Add("TimeoutSeconds must be between 1 and 300");

            if (RetryCount < 0 || RetryCount > 10)
                errors.Add("RetryCount must be between 0 and 10");

            if (errors.Any())
                throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
