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
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ServiceRequestNumber))
                .Replace("{{ReporterName}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterName))
                .Replace("{{ReporterEmail}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterEmail))
                .Replace("{{ReporterPhoneNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ReporterPhoneNumber))
                .Replace("{{Description}}", SOAPHelper.GetValueOrEmpty(requestDTO.Description))
                .Replace("{{BuildingId}}", SOAPHelper.GetValueOrEmpty(requestDTO.BuildingId))
                .Replace("{{LocationId}}", SOAPHelper.GetValueOrEmpty(requestDTO.LocationId))
                .Replace("{{CategoryId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CategoryId))
                .Replace("{{DisciplineId}}", SOAPHelper.GetValueOrEmpty(requestDTO.DisciplineId))
                .Replace("{{PriorityId}}", SOAPHelper.GetValueOrEmpty(requestDTO.PriorityId))
                .Replace("{{InstructionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.InstructionId))
                .Replace("{{ContractId}}", SOAPHelper.GetValueOrEmpty(requestDTO.ContractId))
                .Replace("{{CallerSourceId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CallerSourceId))
                .Replace("{{ScheduledDateUtc}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledDateUtc))
                .Replace("{{RaisedDateUtc}}", SOAPHelper.GetValueOrEmpty(requestDTO.RaisedDateUtc));
            
            _logger.Debug("SOAP envelope created for CreateBreakdownTask");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_BREAKDOWN_TASK,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CreateBreakdownTaskUrl,
                soapActionUrl: _appConfigs.CreateBreakdownTaskSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"CreateBreakdownTask completed with status: {response.StatusCode}");
            
            return response;
        }
    }
}
