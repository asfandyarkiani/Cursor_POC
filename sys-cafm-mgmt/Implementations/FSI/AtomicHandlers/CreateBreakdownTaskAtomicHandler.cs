using CAFMSystem.ConfigModels;
using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.Helpers;
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
            
            _logger.Info($"CreateBreakdownTaskAtomicHandler processing for CallerSourceId: {requestDTO.CallerSourceId}");

            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.CreateBreakdownTask.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{CallerSourceId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CallerSourceId))
                .Replace("{{Comments}}", SOAPHelper.GetValueOrEmpty(requestDTO.Comments))
                .Replace("{{ContactEmail}}", SOAPHelper.GetValueOrEmpty(requestDTO.ContactEmail))
                .Replace("{{ContactName}}", SOAPHelper.GetValueOrEmpty(requestDTO.ContactName))
                .Replace("{{ContactPhone}}", SOAPHelper.GetValueOrEmpty(requestDTO.ContactPhone))
                .Replace("{{BuildingId}}", SOAPHelper.GetValueOrEmpty(requestDTO.BuildingId))
                .Replace("{{CategoryId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CategoryId))
                .Replace("{{DisciplineId}}", SOAPHelper.GetValueOrEmpty(requestDTO.DisciplineId))
                .Replace("{{LocationId}}", SOAPHelper.GetValueOrEmpty(requestDTO.LocationId))
                .Replace("{{PriorityId}}", SOAPHelper.GetValueOrEmpty(requestDTO.PriorityId))
                .Replace("{{LoggedBy}}", SOAPHelper.GetValueOrEmpty(requestDTO.LoggedBy))
                .Replace("{{RaisedDate}}", SOAPHelper.GetValueOrEmpty(requestDTO.RaisedDate))
                .Replace("{{ScheduledDate}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledDate))
                .Replace("{{ScheduledEndTime}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledEndTime))
                .Replace("{{ScheduledStartTime}}", SOAPHelper.GetValueOrEmpty(requestDTO.ScheduledStartTime))
                .Replace("{{Status}}", SOAPHelper.GetValueOrEmpty(requestDTO.Status))
                .Replace("{{SubStatus}}", SOAPHelper.GetValueOrEmpty(requestDTO.SubStatus))
                .Replace("{{ContractId}}", SOAPHelper.GetValueOrEmpty(requestDTO.ContractId))
                .Replace("{{InstructionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.InstructionId));

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
