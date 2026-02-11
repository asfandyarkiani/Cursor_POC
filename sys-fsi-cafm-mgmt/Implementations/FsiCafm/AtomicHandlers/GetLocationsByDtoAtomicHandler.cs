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
    public class GetLocationsByDtoAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetLocationsByDtoAtomicHandler> _logger;
        private readonly CustomSoapClient _soapClient;
        private readonly AppConfigs _appConfigs;
        
        public GetLocationsByDtoAtomicHandler(
            ILogger<GetLocationsByDtoAtomicHandler> logger,
            CustomSoapClient soapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _soapClient = soapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetLocationsByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetLocationsByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"GetLocationsByDtoAtomicHandler processing for UnitCode: {requestDTO.UnitCode}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetLocationsByDto.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{UnitCode}}", SOAPHelper.GetValueOrEmpty(requestDTO.UnitCode));
            
            string apiUrl = $"{_appConfigs.BaseUrl}{_appConfigs.GetLocationsByDtoResourcePath}";
            
            _logger.Debug("SOAP envelope created for GetLocationsByDto");
            
            HttpResponseSnapshot response = await _soapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_LOCATIONS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: apiUrl,
                soapActionUrl: _appConfigs.GetLocationsByDtoSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetLocationsByDto processing completed: {response.StatusCode}");
            
            return response;
        }
    }
}
