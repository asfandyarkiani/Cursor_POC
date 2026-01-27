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
            
            _logger.Info("AuthenticateAtomicHandler processing started");
            requestDTO.ValidateDownStreamRequestParameters();
            
            string soapEnvelope = await MapDtoToSoapEnvelope(requestDTO);
            
            _logger.Debug("SOAP envelope created for Authenticate");
            
            HttpResponseSnapshot response = await _customSoapClient.ExecuteCustomSoapRequestAsync(
                operationName: OperationNames.AUTHENTICATE,
                soapEnvelope: soapEnvelope,
                apiUrl: _appConfigs.CAFMLoginUrl,
                soapActionUrl: _appConfigs.CAFMLoginSoapAction,
                httpMethod: HttpMethod.Post
            );
            
            _logger.Info($"Authenticate processing completed: {response.StatusCode}");
            return response;
        }
        
        private async Task<string> MapDtoToSoapEnvelope(AuthenticateHandlerReqDTO dto)
        {
            string envelopeTemplate = SOAPHelper.LoadSoapEnvelopeTemplate($"{_appConfigs.ProjectNamespace}.SoapEnvelopes.Authenticate.xml");
            
            Dictionary<string, string> authSecrets = await _keyVaultReader.GetAuthSecretsAsync();
            string username = authSecrets.GetValueOrDefault("CAFMUsername", dto.Username);
            string password = authSecrets.GetValueOrDefault("CAFMPassword", dto.Password);
            
            return envelopeTemplate
                .Replace("{{Username}}", username)
                .Replace("{{Password}}", password);
        }
    }
}
