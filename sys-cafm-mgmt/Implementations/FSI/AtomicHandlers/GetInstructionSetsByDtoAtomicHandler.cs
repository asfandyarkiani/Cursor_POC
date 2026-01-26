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
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"GetInstructionSetsByDtoAtomicHandler processing for description: {requestDTO.InstructionDescription}");

            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetInstructionSetsByDto.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{InstructionDescription}}", SOAPHelper.GetValueOrEmpty(requestDTO.InstructionDescription));

            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_INSTRUCTION_SETS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetInstructionSetsByDtoUrl,
                soapActionUrl: _appConfigs.GetInstructionSetsByDtoSoapAction,
                httpMethod: HttpMethod.Post);

            _logger.Info($"GetInstructionSetsByDto completed with status: {response.StatusCode}");
            
            return response;
        }
    }
}
