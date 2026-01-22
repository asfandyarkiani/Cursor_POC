namespace SysCafmMgmt.ConfigModels
{
    /// <summary>
    /// Application configuration for CAFM System Layer
    /// </summary>
    public class AppConfigs
    {
        /// <summary>
        /// CAFM FSI Connection settings
        /// </summary>
        public CafmSettings Cafm { get; set; } = new();

        /// <summary>
        /// General application settings
        /// </summary>
        public AppSettings App { get; set; } = new();
    }

    public class CafmSettings
    {
        /// <summary>
        /// Base URL for CAFM FSI service (e.g., https://devcafm.agfacilities.com)
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Resource path for Login SOAP endpoint
        /// </summary>
        public string LoginResourcePath { get; set; } = "/Evolution/Services/IntegrationService.svc";

        /// <summary>
        /// Resource path for Logout SOAP endpoint
        /// </summary>
        public string LogoutResourcePath { get; set; } = "/Evolution/Services/IntegrationService.svc";

        /// <summary>
        /// Resource path for Breakdown Task operations
        /// </summary>
        public string BreakdownTaskResourcePath { get; set; } = "/Evolution/Services/IntegrationService.svc";

        /// <summary>
        /// SOAP Action for Authenticate
        /// </summary>
        public string SoapActionLogin { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IIntegrationService/Authenticate";

        /// <summary>
        /// SOAP Action for LogOut
        /// </summary>
        public string SoapActionLogout { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IIntegrationService/LogOut";

        /// <summary>
        /// SOAP Action for GetLocationsByDto
        /// </summary>
        public string SoapActionGetLocations { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IIntegrationService/GetLocationsByDto";

        /// <summary>
        /// SOAP Action for GetInstructionSetsByDto
        /// </summary>
        public string SoapActionGetInstructionSets { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IIntegrationService/GetInstructionSetsByDto";

        /// <summary>
        /// SOAP Action for CreateBreakdownTask
        /// </summary>
        public string SoapActionCreateBreakdownTask { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IIntegrationService/CreateBreakdownTask";

        /// <summary>
        /// SOAP Action for GetBreakdownTasksByDto
        /// </summary>
        public string SoapActionGetBreakdownTasks { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IIntegrationService/GetBreakdownTasksByDto";

        /// <summary>
        /// FSI Username - Retrieved from secure configuration
        /// TODO: Retrieve from Azure Key Vault or secure secret store
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// FSI Password - Retrieved from secure configuration
        /// TODO: Retrieve from Azure Key Vault or secure secret store
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Default Contract ID for breakdown tasks
        /// </summary>
        public string DefaultContractId { get; set; } = string.Empty;

        /// <summary>
        /// Caller Source ID for breakdown tasks
        /// </summary>
        public string CallerSourceId { get; set; } = string.Empty;
    }

    public class AppSettings
    {
        /// <summary>
        /// Current environment (dev, qa, stg, prod, dr)
        /// </summary>
        public string Environment { get; set; } = "dev";

        /// <summary>
        /// HTTP client timeout in seconds
        /// </summary>
        public int HttpTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Enable detailed logging
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;
    }
}
