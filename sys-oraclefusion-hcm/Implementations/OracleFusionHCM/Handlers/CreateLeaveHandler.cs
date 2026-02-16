using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using sys_oraclefusion_hcm.Constants;
using sys_oraclefusion_hcm.DTO.CreateLeaveDTO;
using sys_oraclefusion_hcm.DTO.AtomicHandlerDTOs;
using sys_oraclefusion_hcm.DTO.DownstreamDTOs;
using sys_oraclefusion_hcm.Helper;
using sys_oraclefusion_hcm.Implementations.OracleFusionHCM.AtomicHandlers;
using System.Net;

namespace sys_oraclefusion_hcm.Implementations.OracleFusionHCM.Handlers
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
                _logger.Error($"Failed to create leave: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.OFH_LEVCRT_0001,
                    errorDetails: [$"Oracle Fusion HCM returned status {response.StatusCode}. Response: {response.Content}"],
                    stepName: "CreateLeaveHandler.cs / HandleAsync"
                );
            }
            else
            {
                CreateLeaveApiResDTO? apiResponse = RestApiHelper.DeserializeJsonResponse<CreateLeaveApiResDTO>(response.Content!);

                if (apiResponse == null || apiResponse.PersonAbsenceEntryId == 0)
                {
                    throw new NoResponseBodyException(
                        errorDetails: ["Oracle Fusion HCM returned empty or invalid response"],
                        stepName: "CreateLeaveHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    _logger.Info("[System Layer]-Completed Create Leave");

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
                PersonNumber = request.EmployeeNumber.ToString(),
                AbsenceType = request.AbsenceType,
                Employer = request.Employer,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                AbsenceStatusCd = request.AbsenceStatusCode,
                ApprovalStatusCd = request.ApprovalStatusCode,
                StartDateDuration = request.StartDateDuration,
                EndDateDuration = request.EndDateDuration
            };

            return await _createLeaveAtomicHandler.Handle(atomicRequest);
        }
    }
}
