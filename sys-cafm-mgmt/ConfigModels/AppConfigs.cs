using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CAFMSystem.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        public string BaseApiUrl { get; set; } = string.Empty;
        public string AuthUrl { get; set; } = string.Empty;
        public string LogoutUrl { get; set; } = string.Empty;
        public string GetLocationsByDtoUrl { get; set; } = string.Empty;
        public string GetInstructionSetsByDtoUrl { get; set; } = string.Empty;
        public string GetBreakdownTasksByDtoUrl { get; set; } = string.Empty;
        public string CreateBreakdownTaskUrl { get; set; } = string.Empty;
        public string CreateEventUrl { get; set; } = string.Empty;
        public string ProjectNamespace { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;

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
                
            if (string.IsNullOrWhiteSpace(CreateBreakdownTaskUrl))
                errors.Add("CreateBreakdownTaskUrl is required");
            else if (!Uri.TryCreate(CreateBreakdownTaskUrl, UriKind.Absolute, out _))
                errors.Add("CreateBreakdownTaskUrl must be a valid URL");
                
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
