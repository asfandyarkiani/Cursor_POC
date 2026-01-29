namespace SysCafmMgmt.ConfigModels;

/// <summary>
/// Application configuration for CAFM System Layer
/// </summary>
public class AppConfigs
{
    /// <summary>
    /// FSI CAFM API configuration
    /// </summary>
    public FsiConfig Fsi { get; set; } = new();
}

/// <summary>
/// FSI CAFM specific configuration
/// </summary>
public class FsiConfig
{
    /// <summary>
    /// Base URL for FSI CAFM services (e.g., https://devcafm.agfacilities.com)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// FSI Username for authentication
    /// TODO: Move to Azure Key Vault for production
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// FSI Password for authentication
    /// TODO: Move to Azure Key Vault for production
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for Login endpoint
    /// </summary>
    public string LoginResourcePath { get; set; } = "/FSIWebServices/Services/BreakdownAdminService";

    /// <summary>
    /// Resource path for Logout endpoint
    /// </summary>
    public string LogoutResourcePath { get; set; } = "/FSIWebServices/Services/BreakdownAdminService";

    /// <summary>
    /// Resource path for GetLocationsByDto endpoint
    /// </summary>
    public string GetLocationsResourcePath { get; set; } = "/FSIWebServices/Services/LocationService";

    /// <summary>
    /// Resource path for GetInstructionSetsByDto and CreateBreakdownTask endpoint
    /// </summary>
    public string BreakdownTaskResourcePath { get; set; } = "/FSIWebServices/Services/BreakdownAdminService";

    /// <summary>
    /// SOAP Action for Authenticate
    /// </summary>
    public string SoapActionLogin { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IBreakdownAdmin/Authenticate";

    /// <summary>
    /// SOAP Action for LogOut
    /// </summary>
    public string SoapActionLogout { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IBreakdownAdmin/LogOut";

    /// <summary>
    /// SOAP Action for GetLocationsByDto
    /// </summary>
    public string SoapActionGetLocations { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/ILocation/GetLocationsByDto";

    /// <summary>
    /// SOAP Action for GetInstructionSetsByDto
    /// </summary>
    public string SoapActionGetInstructionSets { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IBreakdownAdmin/GetInstructionSetsByDto";

    /// <summary>
    /// SOAP Action for CreateBreakdownTask
    /// </summary>
    public string SoapActionCreateBreakdownTask { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IBreakdownAdmin/CreateBreakdownTask";

    /// <summary>
    /// SOAP Action for GetBreakdownTasksByDto
    /// </summary>
    public string SoapActionGetBreakdownTasks { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/IBreakdownAdmin/GetBreakdownTasksByDto";

    /// <summary>
    /// Default Contract ID for Work Orders
    /// TODO: Configure per environment
    /// </summary>
    public string ContractId { get; set; } = string.Empty;

    /// <summary>
    /// Caller Source ID for EQ+ integration
    /// </summary>
    public string CallerSourceId { get; set; } = "EQ+";
}
