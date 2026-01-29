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
    /// Atomic handler for CreateBreakdownTask CAFM operation.
    /// Creates a new breakdown task/work order in CAFM system.
    /// </summary>
    public class CreateBreakdownTaskAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateBreakdownTaskAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;

        public CreateBreakdownTaskAtomicHandler(
            ILogger<CreateBreakdownTaskAtomicHandler> logger,
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
            CreateBreakdownTaskHandlerReqDTO requestDTO = downStreamRequestDTO as CreateBreakdownTaskHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type. Expected CreateBreakdownTaskHandlerReqDTO.");

            // Validate
            requestDTO.ValidateDownStreamRequestParameters();

            _logger.Info($"CreateBreakdownTaskAtomicHandler processing for CallId: {requestDTO.CallId}");

            // Build SOAP envelope
            string soapEnvelope = BuildCreateBreakdownTaskSoapEnvelope(requestDTO);

            // Execute SOAP request
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_BREAKDOWN_TASK,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CreateBreakdownTaskUrl,
                soapActionUrl: _appConfigs.CreateBreakdownTaskSoapAction,
                httpMethod: HttpMethod.Post
            );

            _logger.Info($"Received CreateBreakdownTask response - Status: {response.StatusCode}");

            return response;
        }

        private string BuildCreateBreakdownTaskSoapEnvelope(CreateBreakdownTaskHandlerReqDTO requestDTO)
        {
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateBreakdownTask.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);

            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{ReporterName}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterName))
                .Replace("{{ReporterEmail}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterEmail))
                .Replace("{{ReporterPhoneNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterPhoneNumber))
                .Replace("{{CallId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CallId))
                .Replace("{{CategoryId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CategoryId))
                .Replace("{{DisciplineId}}", SOAPHelper.GetValueOrEmpty(requestDTO.DisciplineId))
                .Replace("{{PriorityId}}", SOAPHelper.GetValueOrEmpty(requestDTO.PriorityId))
                .Replace("{{BuildingId}}", SOAPHelper.GetValueOrEmpty(requestDTO.BuildingId))
                .Replace("{{LocationId}}", SOAPHelper.GetValueOrEmpty(requestDTO.LocationId))
                .Replace("{{InstructionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.InstructionId))
                .Replace("{{LongDescription}}", SOAPHelper.GetValueOrEmpty(requestDTO.LongDescription))
                .Replace("{{ScheduledDateUtc}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledDateUtc))
                .Replace("{{RaisedDateUtc}}", SOAPHelper.GetValueOrEmpty(requestDTO.RaisedDateUtc))
                .Replace("{{ContractId}}", SOAPHelper.GetValueOrEmpty(requestDTO.ContractId))
                .Replace("{{CallerSourceId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CallerSourceId));

            return soapEnvelope;
        }
    }
}
