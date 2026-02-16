using Core.DTOs;

namespace OracleFusionHcmSystemLayer.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        public string BaseApiUrl { get; set; } = string.Empty;
        public string AbsencesResourcePath { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;

        public void validate()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(BaseApiUrl))
                errors.Add("BaseApiUrl is required");
            else if (!Uri.TryCreate(BaseApiUrl, UriKind.Absolute, out _))
                errors.Add("BaseApiUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(AbsencesResourcePath))
                errors.Add("AbsencesResourcePath is required");
            
            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                errors.Add("TimeoutSeconds must be between 1 and 300");
            
            if (RetryCount < 0 || RetryCount > 10)
                errors.Add("RetryCount must be between 0 and 10");
            
            if (errors.Any())
                throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
