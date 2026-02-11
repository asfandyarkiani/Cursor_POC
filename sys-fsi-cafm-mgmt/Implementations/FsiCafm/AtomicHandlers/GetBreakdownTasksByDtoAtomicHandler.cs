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
    public class GetBreakdownTasksByDtoAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetBreakdownTasksByDtoAtomicHandler> _logger;
        private readonly CustomSoapClient _soapClient;
        private readonly AppConfigs _appConfigs;
        
        public GetBreakdownTasksByDtoAtomicHandler(
            ILogger<GetBreakdownTasksByDtoAtomicHandler> logger,
            CustomSoapClient soapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _soapClient = soapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetBreakdownTasksByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetBreakdownTasksByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"GetBreakdownTasksByDtoAtomicHandler processing for CallId: {requestDTO.CallId}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetBreakdownTasksByDto.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{CallId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CallId));
            
            string apiUrl = $"{_appConfigs.BaseUrl}{_appConfigs.GetBreakdownTasksByDtoResourcePath}";
            
            _logger.Debug("SOAP envelope created for GetBreakdownTasksByDto");
            
            HttpResponseSnapshot response = await _soapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_BREAKDOWN_TASKS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: apiUrl,
                soapActionUrl: _appConfigs.GetBreakdownTasksByDtoSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetBreakdownTasksByDto processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
