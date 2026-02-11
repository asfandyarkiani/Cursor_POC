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
    /// Atomic handler for CAFM Logout SOAP operation
    /// Terminates the session with FSI CAFM system
    /// </summary>
    public class LogoutAtomicHandler : IAtomicHandler<LogoutSoapResponseDTO>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<LogoutAtomicHandler> _logger;
        private readonly AppConfigs _appConfigs;

        public LogoutAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<LogoutAtomicHandler> logger,
            IOptions<AppConfigs> appConfigs)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
        }

        public async Task<LogoutSoapResponseDTO> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            if (downStreamRequestDTO is not LogoutSoapRequestDTO logoutRequest)
            {
                throw new ArgumentException("Invalid request DTO type", nameof(downStreamRequestDTO));
            }

            _logger.Info($"[LogoutAtomicHandler] Starting CAFM logout for session: {logoutRequest.SessionId}");

            try
            {
                // Build SOAP envelope
                string soapEnvelope = BuildLogoutSoapEnvelope(logoutRequest.SessionId!);

                // Prepare HTTP request
                string url = $"{_appConfigs.CafmSettings.BaseUrl}{_appConfigs.CafmSettings.LogoutResourcePath}";
                
                var headers = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("SOAPAction", _appConfigs.CafmSettings.LogoutSoapAction),
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
                    _logger.Warning($"[LogoutAtomicHandler] CAFM logout returned non-success status: {response.StatusCode}");
                    // Logout failures are often non-critical, so we log but don't throw
                    return new LogoutSoapResponseDTO
                    {
                        Success = false,
                        ErrorMessage = $"Logout returned status {response.StatusCode}",
                        RawResponse = responseBody
                    };
                }

                _logger.Info($"[LogoutAtomicHandler] CAFM logout successful");
                return new LogoutSoapResponseDTO
                {
                    Success = true,
                    Message = "Logout successful",
                    RawResponse = responseBody
                };
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, $"[LogoutAtomicHandler] Exception during CAFM logout: {ex.Message}");
                // Logout exceptions are non-critical
                return new LogoutSoapResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"Logout exception: {ex.Message}",
                    RawResponse = ex.ToString()
                };
            }
        }

        private string BuildLogoutSoapEnvelope(string sessionId)
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
}
