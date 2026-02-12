namespace sys_cafm_workorder.ConfigModels;

/// <summary>
/// Root configuration container for all application settings.
/// </summary>
public class AppConfigs
{
    /// <summary>
    /// CAFM (FSI) system configurations.
    /// </summary>
    public CAFMConfig CAFM { get; set; } = new();
}

/// <summary>
/// Configuration for CAFM (FSI) SOAP Web Services.
/// </summary>
public class CAFMConfig
{
    /// <summary>
    /// Base URL for CAFM SOAP Web Services.
    /// Example: https://devcafm.agfacilities.com
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Web Services endpoint path.
    /// Example: /WebServices/evolution
    /// </summary>
    public string WebServicesPath { get; set; } = "/WebServices/evolution";

    /// <summary>
    /// FSI Username for authentication.
    /// TODO: This should be stored in Key Vault and retrieved at runtime.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// FSI Password for authentication.
    /// TODO: This should be stored in Key Vault and retrieved at runtime.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Contract ID for work order creation.
    /// TODO: This should be retrieved from configuration per environment.
    /// </summary>
    public string ContractId { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action configurations for different operations.
    /// </summary>
    public CAFMSoapActions SoapActions { get; set; } = new();

    /// <summary>
    /// Gets the full CAFM Web Services URL.
    /// </summary>
    public string GetFullUrl() => $"{BaseUrl.TrimEnd('/')}{WebServicesPath}";
}

/// <summary>
/// SOAP Action URIs for CAFM operations.
/// </summary>
public class CAFMSoapActions
{
    /// <summary>
    /// SOAP Action for Authenticate operation.
    /// </summary>
    public string Authenticate { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/Authenticate";

    /// <summary>
    /// SOAP Action for Logout operation.
    /// </summary>
    public string Logout { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/Logout";

    /// <summary>
    /// SOAP Action for GetBreakdownTasksByDto operation.
    /// </summary>
    public string GetBreakdownTasksByDto { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/GetBreakdownTasksByDto";

    /// <summary>
    /// SOAP Action for GetLocationsByDto operation.
    /// </summary>
    public string GetLocationsByDto { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/GetLocationsByDto";

    /// <summary>
    /// SOAP Action for GetInstructionSetsByDto operation.
    /// </summary>
    public string GetInstructionSetsByDto { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/GetInstructionSetsByDto";

    /// <summary>
    /// SOAP Action for CreateBreakdownTask operation.
    /// </summary>
    public string CreateBreakdownTask { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/CreateBreakdownTask";

    /// <summary>
    /// SOAP Action for CreateEvent operation.
    /// </summary>
    public string CreateEvent { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/CreateEvent";
}
