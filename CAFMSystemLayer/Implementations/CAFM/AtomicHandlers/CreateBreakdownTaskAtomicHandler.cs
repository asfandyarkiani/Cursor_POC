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
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"CreateBreakdownTaskAtomicHandler processing for ServiceRequestNumber: {requestDTO.ServiceRequestNumber}");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateBreakdownTask.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string technicianElement = SOAPHelper.GetElementOrEmpty("ns:technician", requestDTO.Technician);
            string sourceOrgIdElement = SOAPHelper.GetElementOrEmpty("ns:sourceOrgId", requestDTO.SourceOrgId);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{LocationId}}", SOAPHelper.GetValueOrEmpty(requestDTO.LocationId))
                .Replace("{{InstructionSetId}}", SOAPHelper.GetValueOrEmpty(requestDTO.InstructionSetId))
                .Replace("{{Description}}", SOAPHelper.GetValueOrEmpty(requestDTO.Description))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ServiceRequestNumber))
                .Replace("{{ReporterName}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterName))
                .Replace("{{ReporterEmail}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterEmail))
                .Replace("{{ReporterPhoneNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterPhoneNumber))
                .Replace("{{Status}}", SOAPHelper.GetValueOrEmpty(requestDTO.Status))
                .Replace("{{SubStatus}}", SOAPHelper.GetValueOrEmpty(requestDTO.SubStatus))
                .Replace("{{Priority}}", SOAPHelper.GetValueOrEmpty(requestDTO.Priority))
                .Replace("{{ScheduledDate}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledDate))
                .Replace("{{ScheduledTimeStart}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledTimeStart))
                .Replace("{{ScheduledTimeEnd}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledTimeEnd))
                .Replace("{{TechnicianElement}}", technicianElement)
                .Replace("{{SourceOrgIdElement}}", sourceOrgIdElement);
            
            _logger.Debug("SOAP envelope created for CreateBreakdownTask");
            
            string fullUrl = $"{_appConfigs.CAFMBaseUrl}{_appConfigs.CAFMAuthEndpoint}";
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_BREAKDOWN_TASK,
                soapEnvelope: soapEnvelope,
                apiUrl: fullUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/CreateBreakdownTask",
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"CreateBreakdownTask completed: {response.StatusCode}");
            
            return response;
        }
    }
}
