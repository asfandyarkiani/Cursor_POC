using Core.DTOs;

namespace FsiCafmSystem.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        public string ProjectNamespace { get; set; } = "FsiCafmSystem";
        
        // FSI CAFM Base Configuration
        public string BaseUrl { get; set; } = string.Empty;
        public string LoginResourcePath { get; set; } = string.Empty;
        public string LogoutResourcePath { get; set; } = string.Empty;
        
        // FSI CAFM Operation Resource Paths
        public string GetLocationsByDtoResourcePath { get; set; } = string.Empty;
        public string GetInstructionSetsByDtoResourcePath { get; set; } = string.Empty;
        public string GetBreakdownTasksByDtoResourcePath { get; set; } = string.Empty;
        public string CreateBreakdownTaskResourcePath { get; set; } = string.Empty;
        public string CreateEventResourcePath { get; set; } = string.Empty;
        
        // SOAP Actions
        public string LoginSoapAction { get; set; } = string.Empty;
        public string LogoutSoapAction { get; set; } = string.Empty;
        public string GetLocationsByDtoSoapAction { get; set; } = string.Empty;
        public string GetInstructionSetsByDtoSoapAction { get; set; } = string.Empty;
        public string GetBreakdownTasksByDtoSoapAction { get; set; } = string.Empty;
        public string CreateBreakdownTaskSoapAction { get; set; } = string.Empty;
        public string CreateEventSoapAction { get; set; } = string.Empty;
        
        // Authentication (from KeyVault)
        public string? FsiUsername { get; set; }
        public string? FsiPassword { get; set; }
        
        // Business Configuration
        public string? ContractId { get; set; }
        
        // SMTP Configuration
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool SmtpUseSsl { get; set; } = true;
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string SmtpFromEmail { get; set; } = string.Empty;
        
        // HTTP Client Configuration
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;
        
        public void validate()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(BaseUrl))
                errors.Add("BaseUrl is required");
            else if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
                errors.Add("BaseUrl must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(LoginResourcePath))
                errors.Add("LoginResourcePath is required");
            
            if (string.IsNullOrWhiteSpace(LogoutResourcePath))
                errors.Add("LogoutResourcePath is required");
            
            if (string.IsNullOrWhiteSpace(SmtpHost))
                errors.Add("SmtpHost is required");
            
            if (SmtpPort <= 0 || SmtpPort > 65535)
                errors.Add("SmtpPort must be between 1 and 65535");
            
            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                errors.Add("TimeoutSeconds must be between 1 and 300");
            
            if (RetryCount < 0 || RetryCount > 10)
                errors.Add("RetryCount must be between 0 and 10");
            
            if (errors.Any())
                throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
