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
            
            _logger.Info($"GetBreakdownTasksByDtoAtomicHandler processing for ServiceRequestNumber: {requestDTO.ServiceRequestNumber}");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetBreakdownTasksByDto.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ServiceRequestNumber));
            
            _logger.Debug("SOAP envelope created for GetBreakdownTasksByDto");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_BREAKDOWN_TASKS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetBreakdownTasksByDtoUrl,
                soapActionUrl: _appConfigs.GetBreakdownTasksByDtoSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetBreakdownTasksByDto completed with status: {response.StatusCode}");
            
            return response;
        }
    }
}
