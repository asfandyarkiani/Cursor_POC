using CAFMSystem.ConfigModels;
using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.Helper;
using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CAFMSystem.Implementations.FSI.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for CAFM logout (internal use only - NOT an Azure Function).
    /// Used by CustomAuthenticationMiddleware to end session.
    /// </summary>
    public class LogoutAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<LogoutAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;

        public LogoutAtomicHandler(
            ILogger<LogoutAtomicHandler> logger,
            CustomSoapClient customSoapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            // Cast to concrete type
            LogoutRequestDTO requestDTO = downStreamRequestDTO as LogoutRequestDTO 
                ?? throw new ArgumentException("Invalid DTO type. Expected LogoutRequestDTO.");

            // Validate
            requestDTO.ValidateDownStreamRequestParameters();

            _logger.Info($"LogoutAtomicHandler processing for SessionId: {requestDTO.SessionId?.Substring(0, Math.Min(8, requestDTO.SessionId.Length))}...");

            // Build SOAP envelope
            string soapEnvelope = BuildLogoutSoapEnvelope(requestDTO);

            // Execute SOAP request
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.LOGOUT,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.LogoutUrl,
                soapActionUrl: _appConfigs.LogoutSoapAction,
                httpMethod: HttpMethod.Post
            );

            _logger.Info($"Received logout response - Status: {response.StatusCode}");

            return response;
        }

        private string BuildLogoutSoapEnvelope(LogoutRequestDTO requestDTO)
        {
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Logout.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);

            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId));

            return soapEnvelope;
        }
    }
}
