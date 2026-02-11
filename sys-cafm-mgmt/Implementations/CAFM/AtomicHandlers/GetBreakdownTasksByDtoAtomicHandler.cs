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
            
            _logger.Info($"GetBreakdownTasksByDtoAtomicHandler processing for ServiceRequestNumber: {requestDTO.ServiceRequestNumber}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string? sessionId = RequestContext.GetSessionId();
            
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new InvalidOperationException("SessionId is required for CAFM operations");
            }
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO, sessionId);
            
            _logger.Debug("SOAP envelope created for GetBreakdownTasksByDto");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_BREAKDOWN_TASKS,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetBreakdownTasksByDtoUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetBreakdownTasksByDto",
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetBreakdownTasksByDto completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(GetBreakdownTasksByDtoHandlerReqDTO dto, string sessionId)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetBreakdownTasksByDto.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(sessionId))
                .Replace("{{ServiceRequestNumber}}", SOAPHelper.GetValueOrEmpty(dto.ServiceRequestNumber));
        }
    }
}
