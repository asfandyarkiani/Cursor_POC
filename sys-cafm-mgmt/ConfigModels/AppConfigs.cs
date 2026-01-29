namespace AGI.SystemLayer.CAFM.ConfigModels;

public class AppConfigs
{
    public CAFMSettings CAFMSettings { get; set; } = new();
    public EmailSettings EmailSettings { get; set; } = new();
}

public class CAFMSettings
{
    /// <summary>
    /// Base URL for CAFM API (e.g., https://devcafm.agfacilities.com)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Username for CAFM authentication
    /// TODO: Store in Azure Key Vault
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for CAFM authentication
    /// TODO: Store in Azure Key Vault
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for Login endpoint (e.g., /services/evolution/04/09/EvolutionService.svc)
    /// </summary>
    public string LoginResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action for Login
    /// </summary>
    public string LoginSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for Logout endpoint
    /// </summary>
    public string LogoutResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action for Logout
    /// </summary>
    public string LogoutSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for GetLocationsByDto
    /// </summary>
    public string GetLocationsResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action for GetLocationsByDto
    /// </summary>
    public string GetLocationsSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for GetInstructionSetsByDto
    /// </summary>
    public string GetInstructionSetsResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action for GetInstructionSetsByDto
    /// </summary>
    public string GetInstructionSetsSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for GetBreakdownTasksByDto
    /// </summary>
    public string GetBreakdownTasksResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action for GetBreakdownTasksByDto
    /// </summary>
    public string GetBreakdownTasksSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for CreateBreakdownTask
    /// </summary>
    public string CreateBreakdownTaskResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action for CreateBreakdownTask
    /// </summary>
    public string CreateBreakdownTaskSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// Resource path for CreateEvent/Link task
    /// </summary>
    public string CreateEventResourcePath { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action for CreateEvent
    /// </summary>
    public string CreateEventSoapAction { get; set; } = string.Empty;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Read timeout in seconds
    /// </summary>
    public int ReadTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Session cache expiration in minutes
    /// </summary>
    public int SessionCacheExpirationMinutes { get; set; } = 30;
}

public class EmailSettings
{
    public string ToEmail { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public bool HasAttachment { get; set; } = false;
}
