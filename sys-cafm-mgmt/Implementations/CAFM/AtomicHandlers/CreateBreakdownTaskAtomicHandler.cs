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
            CreateBreakdownTaskHandlerReqDTO requestDTO = downStreamRequestDTO as CreateBreakdownTaskHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"CreateBreakdownTaskAtomicHandler processing for ServiceRequestNumber: {requestDTO.ServiceRequestNumber}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string? sessionId = RequestContext.GetSessionId();
            
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new InvalidOperationException("SessionId is required for CAFM operations");
            }
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO, sessionId);
            
            _logger.Debug("SOAP envelope created for CreateBreakdownTask");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_BREAKDOWN_TASK,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CreateBreakdownTaskUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/CreateBreakdownTask",
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"CreateBreakdownTask completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(CreateBreakdownTaskHandlerReqDTO dto, string sessionId)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateBreakdownTask.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(sessionId))
                .Replace("{{ReporterName}}", SOAPHelper.GetValueOrEmpty(dto.ReporterName))
                .Replace("{{ReporterEmail}}", SOAPHelper.GetValueOrEmpty(dto.ReporterEmail))
                .Replace("{{ReporterPhoneNumber}}", SOAPHelper.GetValueOrEmpty(dto.ReporterPhoneNumber))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(dto.ServiceRequestNumber))
                .Replace("{{CategoryId}}", SOAPHelper.GetValueOrEmpty(dto.CategoryId))
                .Replace("{{DisciplineId}}", SOAPHelper.GetValueOrEmpty(dto.DisciplineId))
                .Replace("{{PriorityId}}", SOAPHelper.GetValueOrEmpty(dto.PriorityId))
                .Replace("{{BuildingId}}", SOAPHelper.GetValueOrEmpty(dto.BuildingId))
                .Replace("{{LocationId}}", SOAPHelper.GetValueOrEmpty(dto.LocationId))
                .Replace("{{InstructionId}}", SOAPHelper.GetValueOrEmpty(dto.InstructionId))
                .Replace("{{Description}}", SOAPHelper.GetValueOrEmpty(dto.Description))
                .Replace("{{ScheduledDateUtc}}", SOAPHelper.GetValueOrEmpty(dto.ScheduledDateUtc))
                .Replace("{{RaisedDateUtc}}", SOAPHelper.GetValueOrEmpty(dto.RaisedDateUtc))
                .Replace("{{ContractId}}", SOAPHelper.GetValueOrEmpty(dto.ContractId))
                .Replace("{{SourceOrgId}}", SOAPHelper.GetValueOrEmpty(dto.SourceOrgId));
        }
    }
}
