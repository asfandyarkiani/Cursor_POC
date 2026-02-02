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

namespace CAFMSystem.Implementations.FSI.AtomicHandlers
{
    /// <summary>
    /// Atomic handler for GetInstructionSetsByDto CAFM operation.
    /// Retrieves instruction set ID based on category name and sub-category.
    /// </summary>
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
            // Cast to concrete type
            GetInstructionSetsByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetInstructionSetsByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type. Expected GetInstructionSetsByDtoHandlerReqDTO.");

            // Validate
            requestDTO.ValidateDownStreamRequestParameters();

            _logger.Info($"GetInstructionSetsByDtoAtomicHandler processing for CategoryName: {requestDTO.CategoryName}, SubCategory: {requestDTO.SubCategory}");

            // Build SOAP envelope
            string soapEnvelope = BuildGetInstructionSetsByDtoSoapEnvelope(requestDTO);

            // Execute SOAP request
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_INSTRUCTION_SETS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetInstructionSetsByDtoUrl,
                soapActionUrl: _appConfigs.GetInstructionSetsByDtoSoapAction,
                httpMethod: HttpMethod.Post
            );

            _logger.Info($"Received GetInstructionSetsByDto response - Status: {response.StatusCode}");

            return response;
        }

        private string BuildGetInstructionSetsByDtoSoapEnvelope(GetInstructionSetsByDtoHandlerReqDTO requestDTO)
        {
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetInstructionSetsByDto.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);

            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{CategoryName}}", SOAPHelper.GetValueOrEmpty(requestDTO.CategoryName))
                .Replace("{{SubCategory}}", SOAPHelper.GetValueOrEmpty(requestDTO.SubCategory));

            return soapEnvelope;
        }
    }
}
