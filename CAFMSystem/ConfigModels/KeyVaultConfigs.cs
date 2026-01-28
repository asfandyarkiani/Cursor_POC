using Core.DTOs;
using System;
using System.Collections.Generic;

namespace CAFMSystem.ConfigModels
{
    /// <summary>
    /// KeyVault configuration for CAFM System Layer.
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
                errors.Add("KeyVault URL is required.");
            else if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
                errors.Add("KeyVault URL must be a valid absolute URL.");

            if (errors.Count > 0)
                throw new InvalidOperationException($"KeyVaultConfigs validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
