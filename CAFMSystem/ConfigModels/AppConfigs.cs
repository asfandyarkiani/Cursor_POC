using Core.DTOs;
using System;
using System.Collections.Generic;

namespace CAFMSystem.ConfigModels
{
    /// <summary>
    /// Application configuration for CAFM System Layer.
    /// Binds to "AppConfigs" section in appsettings.json.
    /// </summary>
    public class AppConfigs : IConfigValidator
    {
        public static string SectionName = "AppConfigs";

        // Environment
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;

        // Base URLs
        public string BaseApiUrl { get; set; } = string.Empty;
        public string LoginUrl { get; set; } = string.Empty;
        public string LogoutUrl { get; set; } = string.Empty;
        public string GetBreakdownTasksByDtoUrl { get; set; } = string.Empty;
        public string GetLocationsByDtoUrl { get; set; } = string.Empty;
        public string GetInstructionSetsByDtoUrl { get; set; } = string.Empty;
        public string CreateBreakdownTaskUrl { get; set; } = string.Empty;
        public string CreateEventUrl { get; set; } = string.Empty;

        // SOAP Actions
        public string LoginSoapAction { get; set; } = string.Empty;
        public string LogoutSoapAction { get; set; } = string.Empty;
        public string GetBreakdownTasksByDtoSoapAction { get; set; } = string.Empty;
        public string GetLocationsByDtoSoapAction { get; set; } = string.Empty;
        public string GetInstructionSetsByDtoSoapAction { get; set; } = string.Empty;
        public string CreateBreakdownTaskSoapAction { get; set; } = string.Empty;
        public string CreateEventSoapAction { get; set; } = string.Empty;

        // Project Settings
        public string ProjectNamespace { get; set; } = "CAFMSystem";
        public string ContractId { get; set; } = string.Empty;
        public string CallerSourceId { get; set; } = string.Empty;

        // HTTP Client Settings
        public int TimeoutSeconds { get; set; } = 50;
        public int RetryCount { get; set; } = 0;

        public void validate()
        {
            List<string> errors = new List<string>();

            // Validate URLs
            if (string.IsNullOrWhiteSpace(BaseApiUrl))
                errors.Add("BaseApiUrl is required.");
            else if (!Uri.TryCreate(BaseApiUrl, UriKind.Absolute, out _))
                errors.Add("BaseApiUrl must be a valid absolute URL.");

            if (string.IsNullOrWhiteSpace(LoginUrl))
                errors.Add("LoginUrl is required.");
            else if (!Uri.TryCreate(LoginUrl, UriKind.Absolute, out _))
                errors.Add("LoginUrl must be a valid absolute URL.");

            if (string.IsNullOrWhiteSpace(LogoutUrl))
                errors.Add("LogoutUrl is required.");
            else if (!Uri.TryCreate(LogoutUrl, UriKind.Absolute, out _))
                errors.Add("LogoutUrl must be a valid absolute URL.");

            if (string.IsNullOrWhiteSpace(CreateBreakdownTaskUrl))
                errors.Add("CreateBreakdownTaskUrl is required.");
            else if (!Uri.TryCreate(CreateBreakdownTaskUrl, UriKind.Absolute, out _))
                errors.Add("CreateBreakdownTaskUrl must be a valid absolute URL.");

            // Validate SOAP Actions
            if (string.IsNullOrWhiteSpace(LoginSoapAction))
                errors.Add("LoginSoapAction is required.");

            if (string.IsNullOrWhiteSpace(CreateBreakdownTaskSoapAction))
                errors.Add("CreateBreakdownTaskSoapAction is required.");

            // Validate Project Settings
            if (string.IsNullOrWhiteSpace(ProjectNamespace))
                errors.Add("ProjectNamespace is required.");

            if (string.IsNullOrWhiteSpace(CallerSourceId))
                errors.Add("CallerSourceId is required.");

            // Validate HTTP Client Settings
            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                errors.Add("TimeoutSeconds must be between 1 and 300.");

            if (RetryCount < 0 || RetryCount > 10)
                errors.Add("RetryCount must be between 0 and 10.");

            if (errors.Count > 0)
                throw new InvalidOperationException($"AppConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
