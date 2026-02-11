using CAFMSystemLayer.ConfigModels;
using CAFMSystemLayer.Constants;
using CAFMSystemLayer.DTO.AtomicHandlerDTOs;
using CAFMSystemLayer.Helper;
using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CAFMSystemLayer.Implementations.CAFM.AtomicHandlers
{
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
            
            _logger.Info("AuthenticateAtomicHandler processing started");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Authenticate.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{Username}}", SOAPHelper.GetValueOrEmpty(requestDTO.Username))
                .Replace("{{Password}}", SOAPHelper.GetValueOrEmpty(requestDTO.Password));
            
            _logger.Debug("SOAP envelope created for authentication");
            
            string fullUrl = $"{_appConfigs.CAFMBaseUrl}{_appConfigs.CAFMAuthEndpoint}";
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.AUTHENTICATE,
                soapEnvelope: soapEnvelope,
                apiUrl: fullUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/Authenticate",
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"Authentication processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
