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
    public class GetLocationsAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetLocationsAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;
        
        public GetLocationsAtomicHandler(
            ILogger<GetLocationsAtomicHandler> logger, 
            CustomSoapClient customSoapClient, 
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetLocationsHandlerReqDTO requestDTO = downStreamRequestDTO as GetLocationsHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"GetLocationsAtomicHandler processing for Property: {requestDTO.PropertyName}, Unit: {requestDTO.UnitCode}");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetLocationsByDto.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{PropertyName}}", SOAPHelper.GetValueOrEmpty(requestDTO.PropertyName))
                .Replace("{{UnitCode}}", SOAPHelper.GetValueOrEmpty(requestDTO.UnitCode));
            
            _logger.Debug("SOAP envelope created for GetLocationsByDto");
            
            string fullUrl = $"{_appConfigs.CAFMBaseUrl}{_appConfigs.CAFMAuthEndpoint}";
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_LOCATIONS,
                soapEnvelope: soapEnvelope,
                apiUrl: fullUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/GetLocationsByDto",
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"GetLocations completed: {response.StatusCode}");
            
            return response;
        }
    }
}
