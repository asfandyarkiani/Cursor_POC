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
    public class GetInstructionSetsAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<GetInstructionSetsAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;
        
        public GetInstructionSetsAtomicHandler(
            ILogger<GetInstructionSetsAtomicHandler> logger, 
            CustomSoapClient customSoapClient, 
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            GetInstructionSetsHandlerReqDTO requestDTO = downStreamRequestDTO as GetInstructionSetsHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"GetInstructionSetsAtomicHandler processing for Category: {requestDTO.CategoryName}, SubCategory: {requestDTO.SubCategory}");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetInstructionSetsByDto.xml";
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = envelopeTemplate
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{CategoryName}}", SOAPHelper.GetValueOrEmpty(requestDTO.CategoryName))
                .Replace("{{SubCategory}}", SOAPHelper.GetValueOrEmpty(requestDTO.SubCategory));
            
            _logger.Debug("SOAP envelope created for GetInstructionSetsByDto");
            
            string fullUrl = $"{_appConfigs.CAFMBaseUrl}{_appConfigs.CAFMAuthEndpoint}";
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_INSTRUCTION_SETS,
                soapEnvelope: soapEnvelope,
                apiUrl: fullUrl,
                soapActionUrl: "http://www.fsi.co.uk/services/evolution/04/09/GetInstructionSetsByDto",
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"GetInstructionSets completed: {response.StatusCode}");
            
            return response;
        }
    }
}
