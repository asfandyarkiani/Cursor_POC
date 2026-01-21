namespace SysNetworkMgmt.ConfigModels
{
    /// <summary>
    /// Application configuration settings for sys-network-mgmt System Layer API.
    /// </summary>
    public class AppConfigs
    {
        /// <summary>
        /// Section name in appsettings.json for binding configuration.
        /// </summary>
        public const string SectionName = "AppConfigs";

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        public string AppName { get; set; } = "sys-network-mgmt";

        /// <summary>
        /// Gets or sets the current environment (dev, qa, stg, prod, dr).
        /// </summary>
        public string Environment { get; set; } = "local";
    }
}
