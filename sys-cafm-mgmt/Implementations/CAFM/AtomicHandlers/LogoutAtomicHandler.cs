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

namespace CAFMSystem.Implementations.CAFM.AtomicHandlers
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
            
            _logger.Debug("SOAP envelope created for logout");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.LOGOUT,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.LogoutUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/LogOut",
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"Logout completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(LogoutHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Logout.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(dto.SessionId));
        }
    }
}
