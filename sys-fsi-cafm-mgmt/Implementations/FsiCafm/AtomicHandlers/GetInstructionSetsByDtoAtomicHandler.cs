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
    public class GetInstructionSetsByDtoAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetInstructionSetsByDtoAtomicHandler> _logger;
        private readonly CustomSoapClient _soapClient;
        private readonly AppConfigs _appConfigs;
        
        public GetInstructionSetsByDtoAtomicHandler(
            ILogger<GetInstructionSetsByDtoAtomicHandler> logger,
            CustomSoapClient soapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _soapClient = soapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetInstructionSetsByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetInstructionSetsByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"GetInstructionSetsByDtoAtomicHandler processing for Description: {requestDTO.Description}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetInstructionSetsByDto.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{Description}}", SOAPHelper.GetValueOrEmpty(requestDTO.Description));
            
            string apiUrl = $"{_appConfigs.BaseUrl}{_appConfigs.GetInstructionSetsByDtoResourcePath}";
            
            _logger.Debug("SOAP envelope created for GetInstructionSetsByDto");
            
            HttpResponseSnapshot response = await _soapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_INSTRUCTION_SETS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: apiUrl,
                soapActionUrl: _appConfigs.GetInstructionSetsByDtoSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetInstructionSetsByDto processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
