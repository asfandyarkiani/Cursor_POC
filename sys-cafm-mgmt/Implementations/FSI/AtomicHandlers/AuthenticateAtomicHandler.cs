using CAFMSystem.ConfigModels;
using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.Helpers;
using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CAFMSystem.Implementations.FSI.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for CAFM authentication.
    /// Internal only - used by middleware, NOT exposed as Azure Function.
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
            AuthenticationRequestDTO requestDTO = downStreamRequestDTO as AuthenticationRequestDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info("AuthenticateAtomicHandler processing CAFM authentication");

            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Authenticate.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = template
                .Replace("{{Username}}", SOAPHelper.GetValueOrEmpty(requestDTO.Username))
                .Replace("{{Password}}", SOAPHelper.GetValueOrEmpty(requestDTO.Password));

            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.AUTHENTICATE,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.LoginUrl,
                soapActionUrl: _appConfigs.LoginSoapAction,
                httpMethod: HttpMethod.Post);

            _logger.Info($"CAFM authentication completed with status: {response.StatusCode}");
            
            return response;
        }
    }
}
