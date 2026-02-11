using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AGI.SystemLayer.CAFM.ConfigModels;
using Core.Middlewares;
using Core.SystemLayer.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic Handler for CAFM Logout operation.
/// Performs a single SOAP call to terminate the CAFM session.
/// </summary>
public class LogoutAtomicHandler : IAtomicHandler
{
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _config;
    private readonly ILogger<LogoutAtomicHandler> _logger;

    public LogoutAtomicHandler(
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> config,
        ILogger<LogoutAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Logs out from CAFM and terminates the session
    /// </summary>
    public async Task LogoutAsync(string sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var soapRequest = BuildLogoutSoapRequest(sessionId);
            var url = $"{_config.CAFMSettings.BaseUrl}{_config.CAFMSettings.LogoutResourcePath}";

            _logger.LogInformation("CAFM Logout: Sending logout request to {Url} for SessionId: {SessionId}", url, sessionId);

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml"),
                new Dictionary<string, string>
                {
                    { "SOAPAction", _config.CAFMSettings.LogoutSoapAction }
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("CAFM Logout: Logout failed with status {StatusCode}. Response: {Response}",
                    response.StatusCode, errorContent);
            }
            else
            {
                _logger.LogInformation("CAFM Logout: Logout successful for SessionId: {SessionId}", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CAFM Logout: Exception during logout for SessionId: {SessionId}", sessionId);
            // Don't throw - logout failure shouldn't fail the request
        }
    }

    private string BuildLogoutSoapRequest(string sessionId)
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:LogOut>
         <ns:sessionId>{System.Security.SecurityElement.Escape(sessionId)}</ns:sessionId>
      </ns:LogOut>
   </soapenv:Body>
</soapenv:Envelope>";
    }
}
