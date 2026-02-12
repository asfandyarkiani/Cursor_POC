using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHCMSystemLayer.ConfigModels;
using OracleFusionHCMSystemLayer.Constants;
using OracleFusionHCMSystemLayer.DTO.AtomicHandlerDTOs;
using OracleFusionHCMSystemLayer.Helper;
using System.Text.Json;

namespace OracleFusionHCMSystemLayer.Implementations.OracleFusionHCM.AtomicHandlers
{
    public class CreateAbsenceAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateAbsenceAtomicHandler> _logger;
        private readonly CustomRestClient _restClient;
        private readonly AppConfigs _appConfigs;
        private readonly KeyVaultReader _keyVaultReader;

        public CreateAbsenceAtomicHandler(
            ILogger<CreateAbsenceAtomicHandler> logger,
            CustomRestClient restClient,
            IOptions<AppConfigs> options,
            KeyVaultReader keyVaultReader)
        {
            _logger = logger;
            _restClient = restClient;
            _appConfigs = options.Value;
            _keyVaultReader = keyVaultReader;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateAbsenceHandlerReqDTO requestDTO = downStreamRequestDTO as CreateAbsenceHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type - expected CreateAbsenceHandlerReqDTO");

            _logger.Info($"Starting CreateAbsence for PersonNumber: {requestDTO.PersonNumber}");

            requestDTO.ValidateDownStreamRequestParameters();

            // Get credentials from KeyVault
            Dictionary<string, string> credentials = await _keyVaultReader.GetOracleFusionCredentialsAsync();
            string username = credentials.GetValueOrDefault(KeyVaultConfigs.ORACLE_FUSION_USERNAME_KEY) ?? string.Empty;
            string password = credentials.GetValueOrDefault(KeyVaultConfigs.ORACLE_FUSION_PASSWORD_KEY) ?? string.Empty;

            // Build full URL
            string fullUrl = RestApiHelper.BuildUrl(_appConfigs.OracleFusionBaseUrl, _appConfigs.OracleFusionAbsencesResourcePath);

            // Build request body
            object requestBody = new
            {
                personNumber = requestDTO.PersonNumber,
                absenceType = requestDTO.AbsenceType,
                employer = requestDTO.Employer,
                startDate = requestDTO.StartDate,
                endDate = requestDTO.EndDate,
                absenceStatusCd = requestDTO.AbsenceStatusCd,
                approvalStatusCd = requestDTO.ApprovalStatusCd,
                startDateDuration = requestDTO.StartDateDuration,
                endDateDuration = requestDTO.EndDateDuration
            };

            _logger.Info($"Calling Oracle Fusion HCM API: {fullUrl}");

            HttpResponseSnapshot response = await _restClient.ExecuteCustomRestRequestAsync(
                operationName: OperationNames.CREATE_ABSENCE,
                apiUrl: fullUrl,
                httpMethod: HttpMethod.Post,
                contentFactory: () => CustomRestClient.CreateJsonContent(requestBody),
                username: username,
                password: password,
                queryParameters: null,
                customHeaders: null
            );

            _logger.Info($"CreateAbsence completed - Status: {response.StatusCode}");

            return response;
        }
    }
}
