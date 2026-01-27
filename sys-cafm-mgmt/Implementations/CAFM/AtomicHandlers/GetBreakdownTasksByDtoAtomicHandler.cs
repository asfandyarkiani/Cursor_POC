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
    public class GetBreakdownTasksByDtoAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetBreakdownTasksByDtoAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;

        public GetBreakdownTasksByDtoAtomicHandler(
            ILogger<GetBreakdownTasksByDtoAtomicHandler> logger,
            CustomSoapClient customSoapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetBreakdownTasksByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetBreakdownTasksByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"GetBreakdownTasksByDtoAtomicHandler: Checking tasks for SR: {requestDTO.ServiceRequestNumber}");
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO);
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_BREAKDOWN_TASKS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetBreakdownTasksByDtoUrl,
                soapActionUrl: _appConfigs.SoapActionGetBreakdownTasks,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetBreakdownTasksByDto completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(GetBreakdownTasksByDtoHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetBreakdownTasksByDto.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(dto.SessionId))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(dto.ServiceRequestNumber));
        }
    }
}
