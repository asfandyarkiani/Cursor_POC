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

namespace CAFMSystem.Implementations.FSI.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for CreateEvent CAFM operation.
    /// Links a recurring event to an existing breakdown task.
    /// </summary>
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
            // Cast to concrete type
            CreateEventHandlerReqDTO requestDTO = downStreamRequestDTO as CreateEventHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type. Expected CreateEventHandlerReqDTO.");

            // Validate
            requestDTO.ValidateDownStreamRequestParameters();

            _logger.Info($"CreateEventAtomicHandler processing for TaskId: {requestDTO.TaskId}");

            // Build SOAP envelope
            string soapEnvelope = BuildCreateEventSoapEnvelope(requestDTO);

            // Execute SOAP request
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_EVENT,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CreateEventUrl,
                soapActionUrl: _appConfigs.CreateEventSoapAction,
                httpMethod: HttpMethod.Post
            );

            _logger.Info($"Received CreateEvent response - Status: {response.StatusCode}");

            return response;
        }

        private string BuildCreateEventSoapEnvelope(CreateEventHandlerReqDTO requestDTO)
        {
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateEvent.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);

            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{TaskId}}", SOAPHelper.GetValueOrEmpty(requestDTO.TaskId))
                .Replace("{{EventType}}", SOAPHelper.GetValueOrEmpty(requestDTO.EventType));

            return soapEnvelope;
        }
    }
}
