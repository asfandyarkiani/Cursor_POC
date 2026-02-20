using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using OracleFusionHcmSystemLayer.Constants;
using OracleFusionHcmSystemLayer.DTO.AtomicHandlerDTOs;
using OracleFusionHcmSystemLayer.DTO.CreateAbsenceDTO;
using OracleFusionHcmSystemLayer.DTO.DownstreamDTOs;
using OracleFusionHcmSystemLayer.Helper;
using OracleFusionHcmSystemLayer.Implementations.OracleFusion.AtomicHandlers;
using System.Net;

namespace OracleFusionHcmSystemLayer.Implementations.OracleFusion.Handlers
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
                _logger.Error($"Failed to create absence: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.OFH_ABSCRT_0001,
                    errorDetails: [$"Oracle Fusion HCM API failed. Status: {response.StatusCode}. Response: {response.Content}"],
                    stepName: "CreateAbsenceHandler.cs / HandleAsync"
                );
            }
            else
            {
                if (string.IsNullOrWhiteSpace(response.Content))
                {
                    throw new NoResponseBodyException(
                        errorDetails: ["Oracle Fusion HCM returned empty response body"],
                        stepName: "CreateAbsenceHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    CreateAbsenceApiResDTO? apiResponse = RestApiHelper.DeserializeJsonResponse<CreateAbsenceApiResDTO>(response.Content);
                    
                    if (apiResponse == null)
                    {
                        throw new DownStreamApiFailureException(
                            statusCode: HttpStatusCode.InternalServerError,
                            error: ErrorConstants.OFH_ABSCRT_0003,
                            errorDetails: ["Failed to deserialize Oracle Fusion HCM response"],
                            stepName: "CreateAbsenceHandler.cs / HandleAsync"
                        );
                    }
                    else
                    {
                        _logger.Info("[System Layer]-Completed Create Absence");
                        
                        return new BaseResponseDTO(
                            message: InfoConstants.CREATE_ABSENCE_SUCCESS,
                            data: CreateAbsenceResDTO.Map(apiResponse),
                            errorCode: null
                        );
                    }
                }
            }
        }

        private async Task<HttpResponseSnapshot> CreateAbsenceInDownstream(CreateAbsenceReqDTO request)
        {
            CreateAbsenceHandlerReqDTO atomicRequest = new CreateAbsenceHandlerReqDTO
            {
                PersonNumber = request.EmployeeNumber,
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
