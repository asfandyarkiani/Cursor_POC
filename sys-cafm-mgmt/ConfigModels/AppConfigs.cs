using Core.DTOs;

namespace sys_cafm_mgmt.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        public string BaseApiUrl { get; set; } = string.Empty;
        public string AuthUrl { get; set; } = string.Empty;
        public string LogoutUrl { get; set; } = string.Empty;
        public string CreateBreakdownTaskUrl { get; set; } = string.Empty;
        public string GetLocationsByDtoUrl { get; set; } = string.Empty;
        public string GetInstructionSetsByDtoUrl { get; set; } = string.Empty;
        public string GetBreakdownTasksByDtoUrl { get; set; } = string.Empty;
        public string CreateEventUrl { get; set; } = string.Empty;
        public string SoapActionAuthenticate { get; set; } = string.Empty;
        public string SoapActionLogout { get; set; } = string.Empty;
        public string SoapActionCreateBreakdownTask { get; set; } = string.Empty;
        public string SoapActionGetLocations { get; set; } = string.Empty;
        public string SoapActionGetInstructions { get; set; } = string.Empty;
        public string SoapActionGetBreakdownTasks { get; set; } = string.Empty;
        public string SoapActionCreateEvent { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool SmtpEnableSsl { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;
        public string ProjectNamespace { get; set; } = string.Empty;

        public void validate()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(BaseApiUrl))
                errors.Add("BaseApiUrl is required");
            else if (!Uri.TryCreate(BaseApiUrl, UriKind.Absolute, out _))
                errors.Add("BaseApiUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(AuthUrl))
                errors.Add("AuthUrl is required");
            else if (!Uri.TryCreate(AuthUrl, UriKind.Absolute, out _))
                errors.Add("AuthUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(LogoutUrl))
                errors.Add("LogoutUrl is required");
            else if (!Uri.TryCreate(LogoutUrl, UriKind.Absolute, out _))
                errors.Add("LogoutUrl must be a valid URL");
            
            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                errors.Add("TimeoutSeconds must be between 1 and 300");
            
            if (RetryCount < 0 || RetryCount > 10)
                errors.Add("RetryCount must be between 0 and 10");
            
            if (string.IsNullOrWhiteSpace(ProjectNamespace))
                errors.Add("ProjectNamespace is required");
            
            if (errors.Any())
                throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
