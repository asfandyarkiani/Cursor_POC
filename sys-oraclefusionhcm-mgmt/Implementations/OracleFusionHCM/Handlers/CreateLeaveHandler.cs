using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.Helpers;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using OracleFusionHCMSystem.Constants;
using OracleFusionHCMSystem.DTO.AtomicHandlerDTOs;
using OracleFusionHCMSystem.DTO.CreateLeaveDTO;
using OracleFusionHCMSystem.DTO.DownstreamDTOs;
using OracleFusionHCMSystem.Implementations.OracleFusionHCM.AtomicHandlers;
using System.Net;

namespace OracleFusionHCMSystem.Implementations.OracleFusionHCM.Handlers
{
    public class CreateLeaveHandler : IBaseHandler<CreateLeaveReqDTO>
    {
        private readonly ILogger<CreateLeaveHandler> _logger;
        private readonly CreateLeaveAtomicHandler _createLeaveAtomicHandler;

        public CreateLeaveHandler(
            ILogger<CreateLeaveHandler> logger,
            CreateLeaveAtomicHandler createLeaveAtomicHandler)
        {
            _logger = logger;
            _createLeaveAtomicHandler = createLeaveAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateLeaveReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Leave");

            HttpResponseSnapshot response = await CreateLeaveInDownstream(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"Oracle Fusion HCM API failed with status: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.HCM_LEVCRT_0001,
                    errorDetails: [$"Oracle Fusion HCM API returned status {response.StatusCode}. Response: {response.Content}"],
                    stepName: "CreateLeaveHandler.cs / HandleAsync"
                );
            }
            else
            {
                CreateLeaveApiResDTO? apiResponse = RestApiHelper.DeserializeJsonResponse<CreateLeaveApiResDTO>(response.Content!);

                if (apiResponse == null || apiResponse.PersonAbsenceEntryId == null || apiResponse.PersonAbsenceEntryId == 0)
                {
                    throw new NoResponseBodyException(
                        error: ErrorConstants.HCM_LEVCRT_0002,
                        errorDetails: ["Oracle Fusion HCM API returned empty or invalid response body."],
                        stepName: "CreateLeaveHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    _logger.Info($"[System Layer]-Completed Create Leave - PersonAbsenceEntryId: {apiResponse.PersonAbsenceEntryId}");

                    return new BaseResponseDTO(
                        message: InfoConstants.CREATE_LEAVE_SUCCESS,
                        data: CreateLeaveResDTO.Map(apiResponse),
                        errorCode: null
                    );
                }
            }
        }

        private async Task<HttpResponseSnapshot> CreateLeaveInDownstream(CreateLeaveReqDTO request)
        {
            CreateLeaveHandlerReqDTO atomicRequest = new CreateLeaveHandlerReqDTO
            {
                PersonNumber = request.EmployeeNumber,
                AbsenceType = request.AbsenceType,
                Employer = request.Employer,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                AbsenceStatusCd = request.AbsenceStatusCode,
                ApprovalStatusCd = request.ApprovalStatusCode,
                StartDateDuration = request.StartDateDuration,
                EndDateDuration = request.EndDateDuration,
                Username = string.Empty,
                Password = string.Empty
            };

            return await _createLeaveAtomicHandler.Handle(atomicRequest);
        }
    }
}
