using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using AGI.SystemLayer.CAFM.ConfigModels;
using AGI.SystemLayer.CAFM.DTOs.Downstream;
using Core.Middlewares;
using Core.SystemLayer.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;

/// <summary>
/// Atomic Handler for CAFM Login operation.
/// Performs a single SOAP call to authenticate with CAFM and retrieve a session ID.
/// </summary>
public class LoginAtomicHandler : IAtomicHandler
{
    private readonly CustomHTTPClient _httpClient;
    private readonly AppConfigs _config;
    private readonly ILogger<LoginAtomicHandler> _logger;

    public LoginAtomicHandler(
        CustomHTTPClient httpClient,
        IOptions<AppConfigs> config,
        ILogger<LoginAtomicHandler> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates with CAFM and returns session ID
    /// </summary>
    public async Task<CAFMAuthenticationResponseDTO> AuthenticateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var soapRequest = BuildLoginSoapRequest(_config.CAFMSettings.Username, _config.CAFMSettings.Password);
            var url = $"{_config.CAFMSettings.BaseUrl}{_config.CAFMSettings.LoginResourcePath}";

            _logger.LogInformation("CAFM Login: Sending authentication request to {Url}", url);

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml"),
                new Dictionary<string, string>
                {
                    { "SOAPAction", _config.CAFMSettings.LoginSoapAction }
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("CAFM Login: Authentication failed with status {StatusCode}. Response: {Response}",
                    response.StatusCode, errorContent);

                return new CAFMAuthenticationResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = $"Login failed with status {response.StatusCode}: {errorContent}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var sessionId = ParseSessionIdFromResponse(responseContent);

            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.LogError("CAFM Login: Failed to parse SessionId from response");
                return new CAFMAuthenticationResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to parse SessionId from login response"
                };
            }

            _logger.LogInformation("CAFM Login: Authentication successful. SessionId: {SessionId}", sessionId);

            return new CAFMAuthenticationResponseDTO
            {
                IsSuccess = true,
                SessionId = sessionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM Login: Exception during authentication");
            return new CAFMAuthenticationResponseDTO
            {
                IsSuccess = false,
                ErrorMessage = $"Exception during login: {ex.Message}"
            };
        }
    }

    private string BuildLoginSoapRequest(string username, string password)
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:Authenticate>
         <ns:loginName>{System.Security.SecurityElement.Escape(username)}</ns:loginName>
         <ns:password>{System.Security.SecurityElement.Escape(password)}</ns:password>
      </ns:Authenticate>
   </soapenv:Body>
</soapenv:Envelope>";
    }

    private string? ParseSessionIdFromResponse(string soapResponse)
    {
        try
        {
            var doc = XDocument.Parse(soapResponse);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";
            XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";

            var sessionIdElement = doc.Descendants(ns + "SessionId").FirstOrDefault();
            return sessionIdElement?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM Login: Error parsing SessionId from SOAP response");
            return null;
        }
    }
}
