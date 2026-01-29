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
    /// Atomic handler for GetBreakdownTasksByDto CAFM operation.
    /// Checks if breakdown task already exists based on CallId.
    /// </summary>
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
            // Cast to concrete type
            GetBreakdownTasksByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetBreakdownTasksByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type. Expected GetBreakdownTasksByDtoHandlerReqDTO.");

            // Validate
            requestDTO.ValidateDownStreamRequestParameters();

            _logger.Info($"GetBreakdownTasksByDtoAtomicHandler processing for CallId: {requestDTO.CallId}");

            // Build SOAP envelope
            string soapEnvelope = BuildGetBreakdownTasksByDtoSoapEnvelope(requestDTO);

            // Execute SOAP request
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_BREAKDOWN_TASKS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetBreakdownTasksByDtoUrl,
                soapActionUrl: _appConfigs.GetBreakdownTasksByDtoSoapAction,
                httpMethod: HttpMethod.Post
            );

            _logger.Info($"Received GetBreakdownTasksByDto response - Status: {response.StatusCode}");

            return response;
        }

        private string BuildGetBreakdownTasksByDtoSoapEnvelope(GetBreakdownTasksByDtoHandlerReqDTO requestDTO)
        {
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetBreakdownTasksByDto.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);

            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{CallId}}", SOAPHelper.GetValueOrEmpty(requestDTO.CallId));

            return soapEnvelope;
        }
    }
}
