using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.Downstream;
using System.Text;

namespace SysCafmMgmt.Implementations.Fsi.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for FSI LogOut SOAP operation
    /// </summary>
    public class LogoutAtomicHandler : IAtomicHandler<bool>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<LogoutAtomicHandler> _logger;
        private readonly AppConfigs _config;

        private const string StepName = "FSI.Logout";

        public LogoutAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<LogoutAtomicHandler> logger,
            IOptions<AppConfigs> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<bool> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            var request = downStreamRequestDTO as LogoutRequestDto
                ?? throw new ArgumentException($"Expected {nameof(LogoutRequestDto)}", nameof(downStreamRequestDTO));

            request.ValidateDownStreamRequestParameters();

            _logger.Info($"Logging out from CAFM FSI system");

            var url = $"{_config.Cafm.BaseUrl}{_config.Cafm.LogoutResourcePath}";
            var soapEnvelope = request.ToSoapEnvelope();

            var headers = new List<Tuple<string, string>>
            {
                new("SOAPAction", _config.Cafm.SoapActionLogout),
                new("Content-Type", "text/xml; charset=utf-8")
            };

            try
            {
                var response = await _httpClient.SendAsync(
                    HttpMethod.Post,
                    url,
                    () => new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"),
                    headers
                );

                if (response.IsSuccessStatusCode)
                {
                    _logger.Info("Successfully logged out from CAFM FSI");
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.Warn($"FSI Logout returned non-success status {response.StatusCode}: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                // Logout failures should not break the flow - log and continue
                _logger.Warn($"FSI Logout failed with exception: {ex.Message}");
                return false;
            }
        }
    }
}
