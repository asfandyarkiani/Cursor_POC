using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sys_cafm_mgmt.ConfigModels;
using sys_cafm_mgmt.Constants;
using sys_cafm_mgmt.DTO.AtomicHandlerDTOs;
using sys_cafm_mgmt.Helper;

namespace sys_cafm_mgmt.Implementations.CAFM.AtomicHandlers
{
    public class AuthenticateAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<AuthenticateAtomicHandler> _logger;
        private readonly CustomSoapClient _customSoapClient;
        private readonly AppConfigs _appConfigs;
        private readonly KeyVaultReader _keyVaultReader;

        public AuthenticateAtomicHandler(
            ILogger<AuthenticateAtomicHandler> logger,
            CustomSoapClient customSoapClient,
            IOptions<AppConfigs> options,
            KeyVaultReader keyVaultReader)
        {
            _logger = logger;
            _customSoapClient = customSoapClient;
            _appConfigs = options.Value;
            _keyVaultReader = keyVaultReader;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            AuthenticateHandlerReqDTO requestDTO = downStreamRequestDTO as AuthenticateHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info("AuthenticateAtomicHandler: Starting authentication");
            
            Dictionary<string, string> authSecrets = await _keyVaultReader.GetAuthSecretsAsync();
            string username = authSecrets.GetValueOrDefault("FSI-Username") ?? string.Empty;
            string password = authSecrets.GetValueOrDefault("FSI-Password") ?? string.Empty;
            
            string soapEnvelope = MapDtoToSoapEnvelope(username, password);
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.AUTHENTICATE,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.AuthUrl,
                soapActionUrl: _appConfigs.SoapActionAuthenticate,
                httpMethod: HttpMethod.Post);
            
            _logger.Info($"Authentication completed - Status: {response.StatusCode}");
            
            return response;
        }

        private string MapDtoToSoapEnvelope(string username, string password)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Authenticate.xml");
            
            return envelopeTemplate
                .Replace("{{Username}}", SOAPHelper.GetValueOrEmpty(username))
                .Replace("{{Password}}", SOAPHelper.GetValueOrEmpty(password));
        }
    }
}
