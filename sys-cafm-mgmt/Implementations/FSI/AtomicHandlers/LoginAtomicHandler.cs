using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.DownStream;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Core.Exceptions;

namespace SysCafmMgmt.Implementations.FSI.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for CAFM Login SOAP operation
    /// Authenticates with FSI CAFM system and returns session ID
    /// </summary>
    public class LoginAtomicHandler : IAtomicHandler<LoginSoapResponseDTO>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<LoginAtomicHandler> _logger;
        private readonly AppConfigs _appConfigs;

        public LoginAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<LoginAtomicHandler> logger,
            IOptions<AppConfigs> appConfigs)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
        }

        public async Task<LoginSoapResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            if (downStreamRequestDTO is not LoginSoapRequestDTO loginRequest)
            {
                throw new ArgumentException("Invalid request DTO type", nameof(downStreamRequestDTO));
            }

            _logger.Info($"[LoginAtomicHandler] Starting CAFM login for user: {loginRequest.Username}");

            try
            {
                // Build SOAP envelope
                string soapEnvelope = BuildLoginSoapEnvelope(loginRequest.Username!, loginRequest.Password!);

                // Prepare HTTP request
                string url = $"{_appConfigs.CafmSettings.BaseUrl}{_appConfigs.CafmSettings.LoginResourcePath}";
                
                var headers = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("SOAPAction", _appConfigs.CafmSettings.LoginSoapAction),
                    new Tuple<string, string>("Content-Type", "text/xml; charset=utf-8")
                };

                // Send SOAP request
                HttpResponseMessage response = await _httpClient.SendAsync(
                    HttpMethod.Post,
                    url,
                    () => new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"),
                    headers
                );

                // Parse response
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"[LoginAtomicHandler] CAFM login failed with status code: {response.StatusCode}");
                    throw new HTTPBaseException(
                        $"CAFM login failed: {response.StatusCode}",
                        "CAFM_LOGIN_FAILED",
                        response.StatusCode,
                        new List<string> { responseBody },
                        "LoginAtomicHandler",
                        true
                    );
                }

                // Parse SOAP response to extract SessionId
                var result = ParseLoginResponse(responseBody);

                _logger.Info($"[LoginAtomicHandler] CAFM login successful, SessionId obtained");
                return result;
            }
            catch (HTTPBaseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[LoginAtomicHandler] Exception during CAFM login: {ex.Message}");
                throw new HTTPBaseException(
                    $"CAFM login exception: {ex.Message}",
                    "CAFM_LOGIN_EXCEPTION",
                    HttpStatusCode.InternalServerError,
                    new List<string> { ex.ToString() },
                    "LoginAtomicHandler",
                    true
                );
            }
        }

        private string BuildLoginSoapEnvelope(string username, string password)
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

        private LoginSoapResponseDTO ParseLoginResponse(string soapResponse)
        {
            try
            {
                XDocument doc = XDocument.Parse(soapResponse);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                var sessionIdElement = doc.Descendants(ns + "SessionId").FirstOrDefault();

                if (sessionIdElement != null && !string.IsNullOrWhiteSpace(sessionIdElement.Value))
                {
                    return new LoginSoapResponseDTO
                    {
                        Success = true,
                        SessionId = sessionIdElement.Value,
                        RawResponse = soapResponse
                    };
                }
                else
                {
                    _logger.Warning($"[LoginAtomicHandler] SessionId not found in response");
                    return new LoginSoapResponseDTO
                    {
                        Success = false,
                        ErrorMessage = "SessionId not found in CAFM login response",
                        RawResponse = soapResponse
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[LoginAtomicHandler] Failed to parse login response: {ex.Message}");
                return new LoginSoapResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"Failed to parse login response: {ex.Message}",
                    RawResponse = soapResponse
                };
            }
        }
    }
}
