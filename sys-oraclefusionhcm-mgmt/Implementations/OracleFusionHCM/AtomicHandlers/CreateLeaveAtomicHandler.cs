using Core.Extensions;
using Core.Helpers;
using Core.Middlewares;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHCMSystem.ConfigModels;
using OracleFusionHCMSystem.Constants;
using OracleFusionHCMSystem.DTO.AtomicHandlerDTOs;
using OracleFusionHCMSystem.Helper;

namespace OracleFusionHCMSystem.Implementations.OracleFusionHCM.AtomicHandlers
{
    public class CreateLeaveAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateLeaveAtomicHandler> _logger;
        private readonly CustomRestClient _customRestClient;
        private readonly AppConfigs _appConfigs;
        private readonly KeyVaultReader _keyVaultReader;

        public CreateLeaveAtomicHandler(
            ILogger<CreateLeaveAtomicHandler> logger,
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
            CreateLeaveHandlerReqDTO requestDTO = downStreamRequestDTO as CreateLeaveHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");

            _logger.Info($"CreateLeaveAtomicHandler processing for Employee: {requestDTO.PersonNumber}");

            requestDTO.ValidateDownStreamRequestParameters();

            string apiUrl = RestApiHelper.BuildUrl(
                _appConfigs.OracleFusionBaseUrl,
                new List<string> { _appConfigs.LeaveResourcePath }
            );

            object requestBody = MapDtoToRequestBody(requestDTO);

            HttpResponseSnapshot response = await _customRestClient.ExecuteCustomRestRequestAsync(
                operationName: OperationNames.CREATE_LEAVE,
                apiUrl: apiUrl,
                httpMethod: HttpMethod.Post,
                contentFactory: () => CustomRestClient.CreateJsonContent(requestBody),
                username: requestDTO.Username,
                password: requestDTO.Password,
                queryParameters: null,
                customHeaders: null
            );

            _logger.Info($"CreateLeaveAtomicHandler completed - Status: {response.StatusCode}");

            return response;
        }

        private object MapDtoToRequestBody(CreateLeaveHandlerReqDTO dto)
        {
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
