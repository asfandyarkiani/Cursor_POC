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
    public class GetBreakdownTasksAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetBreakdownTasksAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;
        
        public GetBreakdownTasksAtomicHandler(
            ILogger<GetBreakdownTasksAtomicHandler> logger, 
            CustomSoapClient customSoapClient, 
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetBreakdownTasksHandlerReqDTO requestDTO = downStreamRequestDTO as GetBreakdownTasksHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"GetBreakdownTasksAtomicHandler processing for ServiceRequestNumber: {requestDTO.ServiceRequestNumber}");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetBreakdownTasksByDto.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(requestDTO.ServiceRequestNumber));
            
            _logger.Debug("SOAP envelope created for GetBreakdownTasksByDto");
            
            string fullUrl = $"{_appConfigs.CAFMBaseUrl}{_appConfigs.CAFMAuthEndpoint}";
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_BREAKDOWN_TASKS,
                soapEnvelope: soapEnvelope,
                apiUrl: fullUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/GetBreakdownTasksByDto",
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"GetBreakdownTasks completed: {response.StatusCode}");
            
            return response;
        }
    }
}
