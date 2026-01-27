using CAFMManagementSystem.ConfigModels;
using CAFMManagementSystem.Constants;
using CAFMManagementSystem.DTO.AtomicHandlerDTOs;
using CAFMManagementSystem.Helper;
using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CAFMManagementSystem.Implementations.FSIConcept.AtomicHandlers
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
            LogoutHandlerReqDTO requestDTO = downStreamRequestDTO as LogoutHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info("LogoutAtomicHandler processing started");
            requestDTO.ValidateDownStreamRequestParameters();
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO);
            
            _logger.Debug("SOAP envelope created for Logout");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.LOGOUT,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CAFMLogoutUrl,
                soapActionUrl: _appConfigs.CAFMLogoutSoapAction,
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"Logout processing completed: {response.StatusCode}");
            return response;
        }
        
        private string MapDtoToSoapEnvelope(LogoutHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Logout.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", dto.SessionId);
        }
    }
}
