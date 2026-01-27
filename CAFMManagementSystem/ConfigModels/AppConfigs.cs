using Core.DTOs;

namespace CAFMManagementSystem.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        public string CAFMBaseUrl { get; set; } = string.Empty;
        public string CAFMLoginUrl { get; set; } = string.Empty;
        public string CAFMLogoutUrl { get; set; } = string.Empty;
        public string CAFMCreateBreakdownTaskUrl { get; set; } = string.Empty;
        public string CAFMGetBreakdownTasksByDtoUrl { get; set; } = string.Empty;
        public string CAFMGetLocationsByDtoUrl { get; set; } = string.Empty;
        public string CAFMGetInstructionSetsByDtoUrl { get; set; } = string.Empty;
        public string CAFMCreateEventUrl { get; set; } = string.Empty;
        public string CAFMLoginSoapAction { get; set; } = string.Empty;
        public string CAFMLogoutSoapAction { get; set; } = string.Empty;
        public string CAFMCreateBreakdownTaskSoapAction { get; set; } = string.Empty;
        public string CAFMGetBreakdownTasksByDtoSoapAction { get; set; } = string.Empty;
        public string CAFMGetLocationsByDtoSoapAction { get; set; } = string.Empty;
        public string CAFMGetInstructionSetsByDtoSoapAction { get; set; } = string.Empty;
        public string CAFMCreateEventSoapAction { get; set; } = string.Empty;
        public string ProjectNamespace { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;
        
        public void validate()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(CAFMBaseUrl))
                errors.Add("CAFMBaseUrl is required");
            else if (!Uri.TryCreate(CAFMBaseUrl, UriKind.Absolute, out _))
                errors.Add("CAFMBaseUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(CAFMLoginUrl))
                errors.Add("CAFMLoginUrl is required");
            else if (!Uri.TryCreate(CAFMLoginUrl, UriKind.Absolute, out _))
                errors.Add("CAFMLoginUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(CAFMLogoutUrl))
                errors.Add("CAFMLogoutUrl is required");
            else if (!Uri.TryCreate(CAFMLogoutUrl, UriKind.Absolute, out _))
                errors.Add("CAFMLogoutUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(CAFMCreateBreakdownTaskUrl))
                errors.Add("CAFMCreateBreakdownTaskUrl is required");
            
            if (string.IsNullOrWhiteSpace(CAFMGetBreakdownTasksByDtoUrl))
                errors.Add("CAFMGetBreakdownTasksByDtoUrl is required");
            
            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                errors.Add("TimeoutSeconds must be between 1 and 300");
            
            if (RetryCount < 0 || RetryCount > 10)
                errors.Add("RetryCount must be between 0 and 10");
            
            if (errors.Any())
                throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
