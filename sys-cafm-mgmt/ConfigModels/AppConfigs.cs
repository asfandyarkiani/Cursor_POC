using Core.DTOs;

namespace CAFMSystem.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        public string ProjectNamespace { get; set; } = "CAFMSystem";
        public string BaseUrl { get; set; } = string.Empty;
        public string LoginUrl { get; set; } = string.Empty;
        public string LogoutUrl { get; set; } = string.Empty;
        public string GetLocationsByDtoUrl { get; set; } = string.Empty;
        public string GetInstructionSetsByDtoUrl { get; set; } = string.Empty;
        public string CreateBreakdownTaskUrl { get; set; } = string.Empty;
        public string GetBreakdownTasksByDtoUrl { get; set; } = string.Empty;
        public string CreateEventUrl { get; set; } = string.Empty;
        
        public string LoginSoapAction { get; set; } = string.Empty;
        public string LogoutSoapAction { get; set; } = string.Empty;
        public string GetLocationsByDtoSoapAction { get; set; } = string.Empty;
        public string GetInstructionSetsByDtoSoapAction { get; set; } = string.Empty;
        public string CreateBreakdownTaskSoapAction { get; set; } = string.Empty;
        public string GetBreakdownTasksByDtoSoapAction { get; set; } = string.Empty;
        public string CreateEventSoapAction { get; set; } = string.Empty;
        
        public string? Username { get; set; }
        public string? Password { get; set; }
        
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;

        public void validate()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(BaseUrl))
                errors.Add("BaseUrl is required");
            else if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
                errors.Add("BaseUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(LoginUrl))
                errors.Add("LoginUrl is required");
            else if (!Uri.TryCreate(LoginUrl, UriKind.Absolute, out _))
                errors.Add("LoginUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(LogoutUrl))
                errors.Add("LogoutUrl is required");
            else if (!Uri.TryCreate(LogoutUrl, UriKind.Absolute, out _))
                errors.Add("LogoutUrl must be a valid URL");
            
            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                errors.Add("TimeoutSeconds must be between 1 and 300");
            
            if (RetryCount < 0 || RetryCount > 10)
                errors.Add("RetryCount must be between 0 and 10");
            
            if (errors.Any())
                throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
