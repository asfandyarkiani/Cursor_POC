using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using FsiCafmSystem.ConfigModels;
using FsiCafmSystem.Constants;
using FsiCafmSystem.DTO.AtomicHandlerDTOs;
using FsiCafmSystem.Helper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FsiCafmSystem.Implementations.FsiCafm.AtomicHandlers
{
    public class AuthenticateAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<AuthenticateAtomicHandler> _logger;
        private readonly CustomSoapClient _soapClient;
        private readonly AppConfigs _appConfigs;
        
        public AuthenticateAtomicHandler(
            ILogger<AuthenticateAtomicHandler> logger,
            CustomSoapClient soapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _soapClient = soapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            AuthenticateHandlerReqDTO requestDTO = downStreamRequestDTO as AuthenticateHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info("AuthenticateAtomicHandler processing started");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Authenticate.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{Username}}", SOAPHelper.GetValueOrEmpty(requestDTO.Username))
                .Replace("{{Password}}", SOAPHelper.GetValueOrEmpty(requestDTO.Password));
            
            string apiUrl = $"{_appConfigs.BaseUrl}{_appConfigs.LoginResourcePath}";
            
            _logger.Debug("SOAP envelope created for authentication");
            
            HttpResponseSnapshot response = await _soapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.AUTHENTICATE,
                soapEnvelope: soapEnvelope,
                apiUrl: apiUrl,
                soapActionUrl: _appConfigs.LoginSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"Authentication processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
