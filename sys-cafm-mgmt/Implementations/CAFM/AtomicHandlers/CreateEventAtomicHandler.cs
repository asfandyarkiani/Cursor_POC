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
    public class CreateEventAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateEventAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;

        public CreateEventAtomicHandler(
            ILogger<CreateEventAtomicHandler> logger,
            CustomSoapClient customSoapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateEventHandlerReqDTO requestDTO = downStreamRequestDTO as CreateEventHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"CreateEventAtomicHandler processing for BreakdownTaskId: {requestDTO.BreakdownTaskId}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string? sessionId = RequestContext.GetSessionId();
            
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new InvalidOperationException("SessionId is required for CAFM operations");
            }
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO, sessionId);
            
            _logger.Debug("SOAP envelope created for CreateEvent");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_EVENT,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CreateEventUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/CreateEvent",
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"CreateEvent completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(CreateEventHandlerReqDTO dto, string sessionId)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateEvent.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(sessionId))
                .Replace("{{BreakdownTaskId}}", SOAPHelper.GetValueOrEmpty(dto.BreakdownTaskId));
        }
    }
}
