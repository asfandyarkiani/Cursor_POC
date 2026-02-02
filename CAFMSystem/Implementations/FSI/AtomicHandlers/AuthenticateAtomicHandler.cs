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
    /// Atomic handler for CAFM authentication (internal use only - NOT an Azure Function).
    /// Used by CustomAuthenticationMiddleware to obtain SessionId.
    /// </summary>
    public class AuthenticateAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<AuthenticateAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;

        public AuthenticateAtomicHandler(
            ILogger<AuthenticateAtomicHandler> logger,
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
            AuthenticationRequestDTO requestDTO = downStreamRequestDTO as AuthenticationRequestDTO 
                ?? throw new ArgumentException("Invalid DTO type. Expected AuthenticationRequestDTO.");

            // Validate
            requestDTO.ValidateDownStreamRequestParameters();

            _logger.Info($"AuthenticateAtomicHandler processing for Username: {requestDTO.Username}");

            // Build SOAP envelope
            string soapEnvelope = BuildAuthenticationSoapEnvelope(requestDTO);

            // Execute SOAP request
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.AUTHENTICATE,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.LoginUrl,
                soapActionUrl: _appConfigs.LoginSoapAction,
                httpMethod: HttpMethod.Post
            );

            _logger.Info($"Received authentication response - Status: {response.StatusCode}");

            return response;
        }

        private string BuildAuthenticationSoapEnvelope(AuthenticationRequestDTO requestDTO)
        {
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Authenticate.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);

            string soapEnvelope = template
                .Replace("{{Username}}", SOAPHelper.GetValueOrEmpty(requestDTO.Username))
                .Replace("{{Password}}", SOAPHelper.GetValueOrEmpty(requestDTO.Password));

            return soapEnvelope;
        }
    }
}
