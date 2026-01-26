using Core.Validators;

namespace CAFMSystemLayer.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        public string CAFMBaseUrl { get; set; } = string.Empty;
        public string CAFMAuthEndpoint { get; set; } = string.Empty;
        public string CAFMUsername { get; set; } = string.Empty;
        public string CAFMPassword { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;
        public string ProjectNamespace { get; set; } = string.Empty;
        
        public void validate()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(CAFMBaseUrl))
                errors.Add("CAFMBaseUrl is required");
            else if (!Uri.TryCreate(CAFMBaseUrl, UriKind.Absolute, out _))
                errors.Add("CAFMBaseUrl must be a valid URL");
                
            if (string.IsNullOrWhiteSpace(CAFMAuthEndpoint))
                errors.Add("CAFMAuthEndpoint is required");
                
            if (string.IsNullOrWhiteSpace(ProjectNamespace))
                errors.Add("ProjectNamespace is required");
                
            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                errors.Add("TimeoutSeconds must be between 1 and 300");
                
            if (RetryCount < 0 || RetryCount > 10)
                errors.Add("RetryCount must be between 0 and 10");
                
            if (errors.Any())
                throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
