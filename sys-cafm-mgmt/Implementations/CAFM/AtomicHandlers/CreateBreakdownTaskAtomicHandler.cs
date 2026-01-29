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
            
            _logger.Info($"CreateBreakdownTaskAtomicHandler: Creating task for SR: {requestDTO.ServiceRequestNumber}");
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO);
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_BREAKDOWN_TASK,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CreateBreakdownTaskUrl,
                soapActionUrl: _appConfigs.SoapActionCreateBreakdownTask,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"CreateBreakdownTask completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(CreateBreakdownTaskHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateBreakdownTask.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(dto.SessionId))
                .Replace("{{ReporterName}}", SOAPHelper.GetValueOrEmpty(dto.ReporterName))
                .Replace("{{ReporterEmail}}", SOAPHelper.GetValueOrEmpty(dto.ReporterEmail))
                .Replace("{{ReporterPhoneNumber}}", SOAPHelper.GetValueOrEmpty(dto.ReporterPhoneNumber))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(dto.ServiceRequestNumber))
                .Replace("{{Description}}", SOAPHelper.GetValueOrEmpty(dto.Description))
                .Replace("{{CategoryId}}", dto.CategoryId.ToString())
                .Replace("{{DisciplineId}}", dto.DisciplineId.ToString())
                .Replace("{{PriorityId}}", dto.PriorityId.ToString())
                .Replace("{{BuildingId}}", dto.BuildingId.ToString())
                .Replace("{{LocationId}}", dto.LocationId.ToString())
                .Replace("{{InstructionId}}", dto.InstructionId.ToString())
                .Replace("{{ScheduledDateUtc}}", SOAPHelper.GetValueOrEmpty(dto.ScheduledDateUtc))
                .Replace("{{RaisedDateUtc}}", SOAPHelper.GetValueOrEmpty(dto.RaisedDateUtc))
                .Replace("{{ContractId}}", dto.ContractId.ToString())
                .Replace("{{CallerSourceId}}", SOAPHelper.GetValueOrEmpty(dto.CallerSourceId));
        }
    }
}
