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
            LogoutRequestDTO requestDTO = downStreamRequestDTO as LogoutRequestDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info("LogoutAtomicHandler processing started");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Logout.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId));
            
            _logger.Debug("SOAP envelope created for logout");
            
            string fullUrl = $"{_appConfigs.CAFMBaseUrl}{_appConfigs.CAFMAuthEndpoint}";
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.LOGOUT,
                soapEnvelope: soapEnvelope,
                apiUrl: fullUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/Logout",
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"Logout processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
