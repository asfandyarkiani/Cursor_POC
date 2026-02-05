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
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CAFMSystem.Implementations.CAFM.AtomicHandlers
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
            
            _logger.Info($"GetLocationsByDtoAtomicHandler processing for UnitCode: {requestDTO.UnitCode}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string? sessionId = RequestContext.GetSessionId();
            
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new InvalidOperationException("SessionId is required for CAFM operations");
            }
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO, sessionId);
            
            _logger.Debug("SOAP envelope created for GetLocationsByDto");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_LOCATIONS,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetLocationsByDtoUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetLocationsByDto",
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetLocationsByDto completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(GetLocationsByDtoHandlerReqDTO dto, string sessionId)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetLocationsByDto.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(sessionId))
                .Replace("{{UnitCode}}", SOAPHelper.GetValueOrEmpty(dto.UnitCode));
        }
    }
}
