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

namespace CAFMSystem.Implementations.FSI.AtomicHandlers
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
            
            _logger.Info($"CreateEventAtomicHandler processing for TaskId: {requestDTO.TaskId}");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateEvent.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{TaskId}}", SOAPHelper.GetValueOrEmpty(requestDTO.TaskId))
                .Replace("{{Comments}}", SOAPHelper.GetValueOrEmpty(requestDTO.Comments));
            
            _logger.Debug("SOAP envelope created for CreateEvent");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_EVENT,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CreateEventUrl,
                soapActionUrl: _appConfigs.CreateEventSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"CreateEvent completed with status: {response.StatusCode}");
            
            return response;
        }
    }
}
