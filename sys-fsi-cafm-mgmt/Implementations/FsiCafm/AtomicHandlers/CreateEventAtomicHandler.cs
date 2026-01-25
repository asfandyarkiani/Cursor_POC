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
    public class CreateEventAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateEventAtomicHandler> _logger;
        private readonly CustomSoapClient _soapClient;
        private readonly AppConfigs _appConfigs;
        
        public CreateEventAtomicHandler(
            ILogger<CreateEventAtomicHandler> logger,
            CustomSoapClient soapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _soapClient = soapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateEventHandlerReqDTO requestDTO = downStreamRequestDTO as CreateEventHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"CreateEventAtomicHandler processing for TaskId: {requestDTO.TaskId}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateEvent.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{TaskId}}", SOAPHelper.GetValueOrEmpty(requestDTO.TaskId))
                .Replace("{{Comments}}", SOAPHelper.GetValueOrEmpty(requestDTO.Comments));
            
            string apiUrl = $"{_appConfigs.BaseUrl}{_appConfigs.CreateEventResourcePath}";
            
            _logger.Debug("SOAP envelope created for CreateEvent");
            
            HttpResponseSnapshot response = await _soapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_EVENT,
                soapEnvelope: soapEnvelope,
                apiUrl: apiUrl,
                soapActionUrl: _appConfigs.CreateEventSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"CreateEvent processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
