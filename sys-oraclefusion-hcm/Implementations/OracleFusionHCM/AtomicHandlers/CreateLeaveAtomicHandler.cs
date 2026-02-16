using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sys_oraclefusion_hcm.ConfigModels;
using sys_oraclefusion_hcm.Constants;
using sys_oraclefusion_hcm.DTO.AtomicHandlerDTOs;
using System.Text;
using System.Text.Json;

namespace sys_oraclefusion_hcm.Implementations.OracleFusionHCM.AtomicHandlers
{
    public class CreateLeaveAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateLeaveAtomicHandler> _logger;
        private readonly CustomRestClient _customRestClient;
        private readonly AppConfigs _appConfigs;

        public CreateLeaveAtomicHandler(
            ILogger<CreateLeaveAtomicHandler> logger,
            CustomRestClient customRestClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customRestClient = customRestClient;
            _appConfigs = options.Value;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateLeaveHandlerReqDTO requestDTO = downStreamRequestDTO as CreateLeaveHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");

            _logger.Info($"Starting CreateLeave for PersonNumber: {requestDTO.PersonNumber}");

            requestDTO.ValidateDownStreamRequestParameters();

            string fullUrl = $"{_appConfigs.OracleFusionBaseUrl}/{_appConfigs.OracleFusionResourcePath}";

            object requestBody = MapDtoToRequestBody(requestDTO);

            HttpResponseSnapshot response = await _customRestClient.ExecuteCustomRestRequestAsync(
                operationName: OperationNames.CREATE_LEAVE,
                apiUrl: fullUrl,
                httpMethod: HttpMethod.Post,
                contentFactory: () => CustomRestClient.CreateJsonContent(requestBody),
                username: requestDTO.Username,
                password: requestDTO.Password,
                queryParameters: null,
                customHeaders: null
            );

            _logger.Info($"CreateLeave completed - Status: {response.StatusCode}");

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
