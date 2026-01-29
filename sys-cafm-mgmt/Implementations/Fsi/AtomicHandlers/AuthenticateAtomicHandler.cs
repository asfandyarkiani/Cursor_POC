using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.Downstream;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SysCafmMgmt.Implementations.Fsi.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for FSI Authenticate SOAP operation
    /// </summary>
    public class AuthenticateAtomicHandler : IAtomicHandler<AuthenticateResponseDto>
    {
        private readonly CustomHTTPClient _httpClient;
        private readonly ILogger<AuthenticateAtomicHandler> _logger;
        private readonly AppConfigs _config;

        private const string StepName = "FSI.Authenticate";

        public AuthenticateAtomicHandler(
            CustomHTTPClient httpClient,
            ILogger<AuthenticateAtomicHandler> logger,
            IOptions<AppConfigs> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<AuthenticateResponseDto> Handle(Core.SystemLayer.DTOs.IDownStreamRequestDTO downStreamRequestDTO)
        {
            var request = downStreamRequestDTO as AuthenticateRequestDto
                ?? throw new ArgumentException($"Expected {nameof(AuthenticateRequestDto)}", nameof(downStreamRequestDTO));

            request.ValidateDownStreamRequestParameters();

            _logger.Info($"Authenticating to CAFM FSI system");

            var url = $"{_config.Cafm.BaseUrl}{_config.Cafm.LoginResourcePath}";
            var soapEnvelope = request.ToSoapEnvelope();

            var headers = new List<Tuple<string, string>>
            {
                new("SOAPAction", _config.Cafm.SoapActionLogin),
                new("Content-Type", "text/xml; charset=utf-8")
            };

            var response = await _httpClient.SendAsync(
                HttpMethod.Post,
                url,
                () => new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"),
                headers
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.Error($"FSI Authentication failed with status {response.StatusCode}: {errorContent}");

                throw new Core.SystemLayer.Exceptions.DownStreamApiFailureException(
                    message: "CAFM FSI Authentication failed",
                    errorCode: "CAFM_AUTH_FAILED",
                    statusCode: response.StatusCode,
                    errorDetails: new List<string> { errorContent },
                    stepName: StepName
                );
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = ParseAuthenticateResponse(responseContent);

            if (!result.IsSuccess)
            {
                _logger.Error("FSI Authentication returned empty session ID");
                throw new Core.SystemLayer.Exceptions.DownStreamApiFailureException(
                    message: "CAFM Log In Failed. CAFM Log In API Responded with Blank Response",
                    errorCode: "CAFM_AUTH_NO_SESSION",
                    statusCode: HttpStatusCode.Unauthorized,
                    stepName: StepName
                );
            }

            _logger.Info($"Successfully authenticated to CAFM FSI. SessionId obtained.");
            return result;
        }

        private AuthenticateResponseDto ParseAuthenticateResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                XNamespace ns1 = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns2 = "http://www.fsi.co.uk/services/evolution/04/09";
                XNamespace ns3 = "http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.Entities.ServiceModel";

                var body = doc.Descendants(ns1 + "Body").FirstOrDefault();
                var authenticateResponse = body?.Descendants(ns2 + "AuthenticateResponse").FirstOrDefault();
                var authenticateResult = authenticateResponse?.Descendants(ns2 + "AuthenticateResult").FirstOrDefault();

                return new AuthenticateResponseDto
                {
                    SessionId = authenticateResult?.Descendants(ns3 + "SessionId").FirstOrDefault()?.Value,
                    OperationResult = authenticateResult?.Descendants(ns3 + "OperationResult").FirstOrDefault()?.Value,
                    EvolutionVersion = authenticateResult?.Descendants(ns3 + "EvolutionVersion").FirstOrDefault()?.Value
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to parse authenticate response: {ex.Message}");
                return new AuthenticateResponseDto();
            }
        }
    }
}
