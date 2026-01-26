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
    public class GetLocationsByDtoAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetLocationsByDtoAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;

        public GetLocationsByDtoAtomicHandler(
            ILogger<GetLocationsByDtoAtomicHandler> logger,
            CustomSoapClient customSoapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetLocationsByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetLocationsByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"GetLocationsByDtoAtomicHandler processing for BarCode: {requestDTO.BarCode}");

            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetLocationsByDto.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{BarCode}}", SOAPHelper.GetValueOrEmpty(requestDTO.BarCode));

            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_LOCATIONS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetLocationsByDtoUrl,
                soapActionUrl: _appConfigs.GetLocationsByDtoSoapAction,
                httpMethod: HttpMethod.Post);

            _logger.Info($"GetLocationsByDto completed with status: {response.StatusCode}");
            
            return response;
        }
    }
}
