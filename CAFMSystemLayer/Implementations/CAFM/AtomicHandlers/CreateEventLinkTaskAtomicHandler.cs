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
    public class CreateEventLinkTaskAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateEventLinkTaskAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;
        
        public CreateEventLinkTaskAtomicHandler(
            ILogger<CreateEventLinkTaskAtomicHandler> logger, 
            CustomSoapClient customSoapClient, 
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateEventLinkTaskHandlerReqDTO requestDTO = downStreamRequestDTO as CreateEventLinkTaskHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"CreateEventLinkTaskAtomicHandler processing for TaskId: {requestDTO.TaskId}");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateEventLinkTask.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{TaskId}}", SOAPHelper.GetValueOrEmpty(requestDTO.TaskId))
                .Replace("{{EventType}}", SOAPHelper.GetValueOrEmpty(requestDTO.EventType))
                .Replace("{{Description}}", SOAPHelper.GetValueOrEmpty(requestDTO.Description));
            
            _logger.Debug("SOAP envelope created for CreateEventLinkTask");
            
            string fullUrl = $"{_appConfigs.CAFMBaseUrl}{_appConfigs.CAFMAuthEndpoint}";
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_EVENT_LINK_TASK,
                soapEnvelope: soapEnvelope,
                apiUrl: fullUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/CreateEvent",
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"CreateEventLinkTask completed: {response.StatusCode}");
            
            return response;
        }
    }
}
