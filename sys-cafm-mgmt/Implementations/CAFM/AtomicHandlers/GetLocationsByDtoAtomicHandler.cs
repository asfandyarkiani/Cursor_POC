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
            
            _logger.Info($"GetLocationsByDtoAtomicHandler: Getting locations for Property: {requestDTO.PropertyName}, Unit: {requestDTO.UnitCode}");
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO);
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_LOCATIONS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetLocationsByDtoUrl,
                soapActionUrl: _appConfigs.SoapActionGetLocations,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetLocationsByDto completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(GetLocationsByDtoHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetLocationsByDto.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(dto.SessionId))
                .Replace("{{PropertyName}}", SOAPHelper.GetValueOrEmpty(dto.PropertyName))
                .Replace("{{UnitCode}}", SOAPHelper.GetValueOrEmpty(dto.UnitCode));
        }
    }
}
