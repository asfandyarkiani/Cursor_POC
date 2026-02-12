using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using OracleFusionHCMSystemLayer.Constants;
using OracleFusionHCMSystemLayer.DTO.AtomicHandlerDTOs;
using OracleFusionHCMSystemLayer.DTO.CreateAbsenceDTO;
using OracleFusionHCMSystemLayer.DTO.DownstreamDTOs;
using OracleFusionHCMSystemLayer.Helper;
using OracleFusionHCMSystemLayer.Implementations.OracleFusionHCM.AtomicHandlers;
using System.Net;

namespace OracleFusionHCMSystemLayer.Implementations.OracleFusionHCM.Handlers
{
    public class CreateAbsenceHandler : IBaseHandler<CreateAbsenceReqDTO>
    {
        private readonly ILogger<CreateAbsenceHandler> _logger;
        private readonly CreateAbsenceAtomicHandler _createAbsenceAtomicHandler;

        public CreateAbsenceHandler(
            ILogger<CreateAbsenceHandler> logger,
            CreateAbsenceAtomicHandler createAbsenceAtomicHandler)
        {
            _logger = logger;
            _createAbsenceAtomicHandler = createAbsenceAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateAbsenceReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Absence");

            HttpResponseSnapshot response = await CreateAbsenceInDownstream(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"Oracle Fusion HCM API call failed - Status: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.OFH_ABSCRT_0001,
                    errorDetails: [$"Oracle Fusion HCM returned status {response.StatusCode}. Response: {response.Content}"],
                    stepName: "CreateAbsenceHandler.cs / HandleAsync"
                );
            }
            else
            {
                CreateAbsenceApiResDTO? apiResponse = RestApiHelper.DeserializeJsonResponse<CreateAbsenceApiResDTO>(response.Content!);

                if (apiResponse == null || apiResponse.PersonAbsenceEntryId == null)
                {
                    throw new NoResponseBodyException(
                        errorDetails: ["Oracle Fusion HCM returned success but response body is missing or invalid"],
                        stepName: "CreateAbsenceHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    _logger.Info($"[System Layer]-Completed - PersonAbsenceEntryId: {apiResponse.PersonAbsenceEntryId}");

                    return new BaseResponseDTO(
                        message: InfoConstants.CREATE_ABSENCE_SUCCESS,
                        data: CreateAbsenceResDTO.Map(apiResponse),
                        errorCode: null
                    );
                }
            }
        }

        private async Task<HttpResponseSnapshot> CreateAbsenceInDownstream(CreateAbsenceReqDTO request)
        {
            CreateAbsenceHandlerReqDTO atomicRequest = new CreateAbsenceHandlerReqDTO
            {
                // Map from API request to Oracle Fusion format (using map field names)
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

            return await _createAbsenceAtomicHandler.Handle(atomicRequest);
        }
    }
}
