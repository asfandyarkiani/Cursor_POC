using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHcmSystemLayer.ConfigModels;
using OracleFusionHcmSystemLayer.Constants;
using OracleFusionHcmSystemLayer.DTO.AtomicHandlerDTOs;
using OracleFusionHcmSystemLayer.Helper;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace OracleFusionHcmSystemLayer.Implementations.OracleFusion.AtomicHandlers
{
    public class CreateAbsenceAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateAbsenceAtomicHandler> _logger;
        private readonly CustomRestClient _customRestClient;
        private readonly AppConfigs _appConfigs;
        private readonly KeyVaultReader _keyVaultReader;

        public CreateAbsenceAtomicHandler(
            ILogger<CreateAbsenceAtomicHandler> logger,
            CustomRestClient customRestClient,
            IOptions<AppConfigs> options,
            KeyVaultReader keyVaultReader)
        {
            _logger = logger;
            _customRestClient = customRestClient;
            _appConfigs = options.Value;
            _keyVaultReader = keyVaultReader;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateAbsenceHandlerReqDTO requestDTO = downStreamRequestDTO as CreateAbsenceHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"Starting CreateAbsence for PersonNumber: {requestDTO.PersonNumber}");
            
            requestDTO.ValidateDownStreamRequestParameters();

            // Get credentials from KeyVault if not provided
            string username = requestDTO.Username ?? _appConfigs.Username ?? string.Empty;
            string password = requestDTO.Password;
            
            if (string.IsNullOrEmpty(password))
            {
                Dictionary<string, string> secrets = await _keyVaultReader.GetSecretsAsync(
                    new List<string> { "OracleFusionHcmPassword" }
                );
                password = secrets.GetValueOrDefault("OracleFusionHcmPassword", string.Empty);
            }

            // Build full URL
            string fullUrl = RestApiHelper.BuildUrl(
                _appConfigs.BaseApiUrl,
                new List<string> { _appConfigs.AbsencesResourcePath }
            );

            // Build request body using map field names (AUTHORITATIVE)
            object requestBody = MapDtoToRequestBody(requestDTO);

            _logger.Info($"Calling Oracle Fusion HCM API: {fullUrl}");

            HttpResponseSnapshot response = await _customRestClient.ExecuteCustomRestRequestAsync(
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

        private object MapDtoToRequestBody(CreateAbsenceHandlerReqDTO dto)
        {
            // Map field names are AUTHORITATIVE (from Map Analysis Step 1d)
            // employeeNumber → personNumber
            // absenceStatusCode → absenceStatusCd
            // approvalStatusCode → approvalStatusCd
            return new
            {
                personNumber = dto.PersonNumber,
                absenceType = dto.AbsenceType,
                employer = dto.Employer,
                startDate = dto.StartDate,
                endDate = dto.EndDate,
                absenceStatusCd = dto.AbsenceStatusCd,
                approvalStatusCd = dto.ApprovalStatusCd,
                startDateDuration = dto.StartDateDuration,
                endDateDuration = dto.EndDateDuration
            };
        }
    }
}
