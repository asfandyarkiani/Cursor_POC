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

namespace CAFMSystem.Implementations.FSI.AtomicHandlers
{
    public class AuthenticateCAFMAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<AuthenticateCAFMAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;

        public AuthenticateCAFMAtomicHandler(
            ILogger<AuthenticateCAFMAtomicHandler> logger,
            CustomSoapClient customSoapClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            AuthenticateCAFMHandlerReqDTO requestDTO = downStreamRequestDTO as AuthenticateCAFMHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"AuthenticateCAFMAtomicHandler processing for Username: {requestDTO.Username}");
            
            string resourceName = $"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Authenticate.xml";
            string template = SOAPHelper.LoadSoapEnvelopeTemplate(resourceName);
            
            string soapEnvelope = template
                .Replace("{{Username}}", SOAPHelper.GetValueOrEmpty(requestDTO.Username))
                .Replace("{{Password}}", SOAPHelper.GetValueOrEmpty(requestDTO.Password));
            
            _logger.Debug("SOAP envelope created for authentication");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.AUTHENTICATE,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.LoginUrl,
                soapActionUrl: _appConfigs.LoginSoapAction,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"Authentication completed with status: {response.StatusCode}");
            
            return response;
        }
    }
}
