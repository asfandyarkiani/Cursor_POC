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
    public class GetInstructionSetsByDtoAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetInstructionSetsByDtoAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;

        public GetInstructionSetsByDtoAtomicHandler(
            ILogger<GetInstructionSetsByDtoAtomicHandler> logger,
            CustomSoapClient customSoapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetInstructionSetsByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetInstructionSetsByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"GetInstructionSetsByDtoAtomicHandler processing for SubCategory: {requestDTO.SubCategory}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string? sessionId = RequestContext.GetSessionId();
            
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new InvalidOperationException("SessionId is required for CAFM operations");
            }
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO, sessionId);
            
            _logger.Debug("SOAP envelope created for GetInstructionSetsByDto");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_INSTRUCTION_SETS,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetInstructionSetsByDtoUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/IEvolutionService/GetInstructionSetsByDto",
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"GetInstructionSetsByDto completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(GetInstructionSetsByDtoHandlerReqDTO dto, string sessionId)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetInstructionSetsByDto.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(sessionId))
                .Replace("{{SubCategory}}", SOAPHelper.GetValueOrEmpty(dto.SubCategory));
        }
    }
}
