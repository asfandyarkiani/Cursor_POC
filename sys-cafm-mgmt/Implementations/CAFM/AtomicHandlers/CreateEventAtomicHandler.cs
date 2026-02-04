using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sys_cafm_mgmt.ConfigModels;
using sys_cafm_mgmt.Constants;
using sys_cafm_mgmt.DTO.AtomicHandlerDTOs;
using sys_cafm_mgmt.Helper;

namespace sys_cafm_mgmt.Implementations.CAFM.AtomicHandlers
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
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"CreateEventAtomicHandler: Creating event for BreakdownTask: {requestDTO.BreakdownTaskId}");
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO);
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_EVENT,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CreateEventUrl,
                soapActionUrl: _appConfigs.SoapActionCreateEvent,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"CreateEvent completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(CreateEventHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateEvent.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(dto.SessionId))
                .Replace("{{BreakdownTaskId}}", dto.BreakdownTaskId.ToString())
                .Replace("{{EventDescription}}", SOAPHelper.GetValueOrEmpty(dto.EventDescription));
        }
    }
}
