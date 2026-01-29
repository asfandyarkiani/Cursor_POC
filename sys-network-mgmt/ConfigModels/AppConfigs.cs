namespace SysNetworkMgmt.ConfigModels
{
    /// <summary>
    /// Application configuration settings for the Network Management System Layer.
    /// </summary>
    public class AppConfigs
    {
        /// <summary>
        /// Configuration section name for binding.
        /// </summary>
        public const string SectionName = "AppConfigs";

        /// <summary>
        /// Environment name (dev, qa, stg, prod, dr).
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Application name for logging and identification.
        /// </summary>
        public string? ApplicationName { get; set; }

        /// <summary>
        /// Application version.
        /// </summary>
        public string? Version { get; set; }

        // TODO: Add additional configuration properties as needed for downstream API calls
        // Example:
        // public string? NetworkServiceBaseUrl { get; set; }
        // public string? ApiKey { get; set; }
    }
}
