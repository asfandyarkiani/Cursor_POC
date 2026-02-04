using CAFMManagementSystem.ConfigModels;
using CAFMManagementSystem.Constants;
using CAFMManagementSystem.DTO.AtomicHandlerDTOs;
using CAFMManagementSystem.Helper;
using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CAFMManagementSystem.Implementations.FSIConcept.AtomicHandlers
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
            
            _logger.Info($"CreateBreakdownTaskAtomicHandler processing for CallId: {requestDTO.CallId}");
            requestDTO.ValidateDownStreamRequestParameters();
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO);
            
            _logger.Debug("SOAP envelope created for CreateBreakdownTask");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.CREATE_BREAKDOWN_TASK,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CAFMCreateBreakdownTaskUrl,
                soapActionUrl: _appConfigs.CAFMCreateBreakdownTaskSoapAction,
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"CreateBreakdownTask completed: {response.StatusCode}");
            return response;
        }
        
        private string MapDtoToSoapEnvelope(CreateBreakdownTaskHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateBreakdownTask.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", dto.SessionId)
                .Replace("{{ReporterName}}", dto.ReporterName ?? string.Empty)
                .Replace("{{ReporterEmail}}", dto.ReporterEmail ?? string.Empty)
                .Replace("{{ReporterPhoneNumber}}", dto.ReporterPhoneNumber ?? string.Empty)
                .Replace("{{CallId}}", dto.CallId)
                .Replace("{{CategoryId}}", dto.CategoryId ?? string.Empty)
                .Replace("{{DisciplineId}}", dto.DisciplineId ?? string.Empty)
                .Replace("{{PriorityId}}", dto.PriorityId ?? string.Empty)
                .Replace("{{BuildingId}}", dto.BuildingId ?? string.Empty)
                .Replace("{{LocationId}}", dto.LocationId ?? string.Empty)
                .Replace("{{InstructionId}}", dto.InstructionId ?? string.Empty)
                .Replace("{{LongDescription}}", dto.LongDescription ?? string.Empty)
                .Replace("{{ScheduledDateUtc}}", dto.ScheduledDateUtc ?? string.Empty)
                .Replace("{{RaisedDateUtc}}", dto.RaisedDateUtc)
                .Replace("{{ContractId}}", dto.ContractId ?? string.Empty)
                .Replace("{{CallerSourceId}}", dto.CallerSourceId ?? string.Empty);
        }
    }
}
