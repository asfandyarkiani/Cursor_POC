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
    public class LogoutAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<LogoutAtomicHandler> _logger;
        private readonly CustomSoapClient _soapClient;
        private readonly AppConfigs _appConfigs;
        
        public LogoutAtomicHandler(
            ILogger<LogoutAtomicHandler> logger,
            CustomSoapClient soapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _soapClient = soapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            LogoutHandlerReqDTO requestDTO = downStreamRequestDTO as LogoutHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info("LogoutAtomicHandler processing started");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Logout.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId));
            
            string apiUrl = $"{_appConfigs.BaseUrl}{_appConfigs.LogoutResourcePath}";
            
            _logger.Debug("SOAP envelope created for logout");
            
            HttpResponseSnapshot response = await _soapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.LOGOUT,
                soapEnvelope: soapEnvelope,
                apiUrl: apiUrl,
                soapActionUrl: _appConfigs.LogoutSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"Logout processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
