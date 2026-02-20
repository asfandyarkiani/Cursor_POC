using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHcm.ConfigModels;
using OracleFusionHcm.Constants;
using OracleFusionHcm.DTO.AtomicHandlerDTOs;
using System.Text.Json;

namespace OracleFusionHcm.Implementations.OracleFusion.AtomicHandlers
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

            _logger.Info($"CreateLeaveAtomicHandler processing for PersonNumber: {requestDTO.PersonNumber}");

            requestDTO.ValidateDownStreamRequestParameters();

            string apiUrl = $"{_appConfigs.BaseApiUrl}/{_appConfigs.ResourcePath}";
            string username = _appConfigs.Username;
            string password = _appConfigs.Password;

            object requestBody = MapDtoToRequestBody(requestDTO);

            HttpResponseSnapshot response = await _customRestClient.ExecuteCustomRestRequestAsync(
                operationName: OperationNames.CREATE_LEAVE,
                apiUrl: apiUrl,
                httpMethod: HttpMethod.Post,
                contentFactory: () => CustomRestClient.CreateJsonContent(requestBody),
                username: username,
                password: password,
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
