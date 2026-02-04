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
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO);
            
            _logger.Debug("SOAP envelope created for CreateEvent");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_EVENT,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CAFMCreateEventUrl,
                soapActionUrl: _appConfigs.CAFMCreateEventSoapAction,
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"CreateEvent completed: {response.StatusCode}");
            return response;
        }
        
        private string MapDtoToSoapEnvelope(CreateEventHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateEvent.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", dto.SessionId)
                .Replace("{{BreakdownTaskId}}", dto.BreakdownTaskId);
        }
    }
}
