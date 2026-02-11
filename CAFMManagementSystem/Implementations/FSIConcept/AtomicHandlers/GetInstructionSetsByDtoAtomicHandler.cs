using CAFMManagementSystem.ConfigModels;
using CAFMManagementSystem.Constants;
using CAFMManagementSystem.DTO.AtomicHandlerDTOs;
using CAFMManagementSystem.Helper;
using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CAFMManagementSystem.Implementations.FSIConcept.AtomicHandlers
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
            
            _logger.Info("GetInstructionSetsByDtoAtomicHandler processing started");
            requestDTO.ValidateDownStreamRequestParameters();
            
            string soapEnvelope = MapDtoToSoapEnvelope(requestDTO);
            
            _logger.Debug("SOAP envelope created for GetInstructionSetsByDto");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_INSTRUCTION_SETS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CAFMGetInstructionSetsByDtoUrl,
                soapActionUrl: _appConfigs.CAFMGetInstructionSetsByDtoSoapAction,
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"GetInstructionSetsByDto completed: {response.StatusCode}");
            return response;
        }
        
        private string MapDtoToSoapEnvelope(GetInstructionSetsByDtoHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetInstructionSetsByDto.xml");
            
            return envelopeTemplate
                .Replace("{{SessionId}}", dto.SessionId)
                .Replace("{{CategoryName}}", dto.CategoryName ?? string.Empty)
                .Replace("{{SubCategory}}", dto.SubCategory ?? string.Empty);
        }
    }
}
