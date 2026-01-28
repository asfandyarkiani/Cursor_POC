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
    /// Atomic handler for GetLocationsByDto CAFM operation.
    /// Retrieves location and building IDs based on property name and unit code.
    /// </summary>
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
            // Cast to concrete type
            GetLocationsByDtoHandlerReqDTO requestDTO = downStreamRequestDTO as GetLocationsByDtoHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type. Expected GetLocationsByDtoHandlerReqDTO.");

            // Validate
            requestDTO.ValidateDownStreamRequestParameters();

            _logger.Info($"GetLocationsByDtoAtomicHandler processing for PropertyName: {requestDTO.PropertyName}, UnitCode: {requestDTO.UnitCode}");

            // Build SOAP envelope
            string soapEnvelope = BuildGetLocationsByDtoSoapEnvelope(requestDTO);

            // Execute SOAP request
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.GET_LOCATIONS_BY_DTO,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.GetLocationsByDtoUrl,
                soapActionUrl: _appConfigs.GetLocationsByDtoSoapAction,
                httpMethod: HttpMethod.Post
            );

            _logger.Info($"Received GetLocationsByDto response - Status: {response.StatusCode}");

            return response;
        }

        private string BuildGetLocationsByDtoSoapEnvelope(GetLocationsByDtoHandlerReqDTO requestDTO)
        {
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.GetLocationsByDto.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);

            string soapEnvelope = template
                .Replace("{{SessionId}}", SOAPHelper.GetValueOrEmpty(requestDTO.SessionId))
                .Replace("{{PropertyName}}", SOAPHelper.GetValueOrEmpty(requestDTO.PropertyName))
                .Replace("{{UnitCode}}", SOAPHelper.GetValueOrEmpty(requestDTO.UnitCode));

            return soapEnvelope;
        }
    }
}
