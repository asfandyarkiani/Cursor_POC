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
    public class CreateBreakdownTaskAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateBreakdownTaskAtomicHandler> _logger;
        private readonly CustomSoapClient _soapClient;
        private readonly AppConfigs _appConfigs;
        
        public CreateBreakdownTaskAtomicHandler(
            ILogger<CreateBreakdownTaskAtomicHandler> logger,
            CustomSoapClient soapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _soapClient = soapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateBreakdownTaskHandlerReqDTO requestDTO = downStreamRequestDTO as CreateBreakdownTaskHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"CreateBreakdownTaskAtomicHandler processing for ServiceRequestNumber: {requestDTO.ServiceRequestNumber}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateBreakdownTask.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{BuildingId}}", SOAPHelper.GetValueOrEmpty(requestDTO.BuildingId))
                .Replace("{{LocationId}}", SOAPHelper.GetValueOrEmpty(requestDTO.LocationId))
                .Replace("{{CategoryId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CategoryId))
                .Replace("{{DisciplineId}}", SOAPHelper.GetValueOrEmpty(requestDTO.DisciplineId))
                .Replace("{{PriorityId}}", SOAPHelper.GetValueOrEmpty(requestDTO.PriorityId))
                .Replace("{{InstructionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.InstructionId))
                .Replace("{{Description}}", SOAPHelper.GetValueOrEmpty(requestDTO.Description))
                .Replace("{{ReporterName}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterName))
                .Replace("{{ReporterEmail}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterEmail))
                .Replace("{{ReporterPhoneNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterPhoneNumber))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ServiceRequestNumber))
                .Replace("{{PropertyName}}", SOAPHelper.GetValueOrEmpty(requestDTO.PropertyName))
                .Replace("{{Technician}}", SOAPHelper.GetValueOrEmpty(requestDTO.Technician))
                .Replace("{{SourceOrgId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SourceOrgId))
                .Replace("{{Status}}", SOAPHelper.GetValueOrEmpty(requestDTO.Status))
                .Replace("{{SubStatus}}", SOAPHelper.GetValueOrEmpty(requestDTO.SubStatus))
                .Replace("{{Priority}}", SOAPHelper.GetValueOrEmpty(requestDTO.Priority))
                .Replace("{{ScheduledDate}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledDate))
                .Replace("{{ScheduledTimeStart}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledTimeStart))
                .Replace("{{ScheduledTimeEnd}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledTimeEnd))
                .Replace("{{RaisedDateUtc}}", SOAPHelper.GetValueOrEmpty(requestDTO.RaisedDateUtc));
            
            string apiUrl = $"{_appConfigs.BaseUrl}{_appConfigs.CreateBreakdownTaskResourcePath}";
            
            _logger.Debug("SOAP envelope created for CreateBreakdownTask");
            
            HttpResponseSnapshot response = await _soapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_BREAKDOWN_TASK,
                soapEnvelope: soapEnvelope,
                apiUrl: apiUrl,
                soapActionUrl: _appConfigs.CreateBreakdownTaskSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"CreateBreakdownTask processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
