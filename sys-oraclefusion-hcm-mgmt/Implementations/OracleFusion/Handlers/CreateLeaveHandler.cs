using Core.DTOs;
using Core.Extensions;
using Core.Helpers;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using OracleFusionHcmMgmt.Constants;
using OracleFusionHcmMgmt.DTO.AtomicHandlerDTOs;
using OracleFusionHcmMgmt.DTO.CreateLeaveDTO;
using OracleFusionHcmMgmt.DTO.DownstreamDTOs;
using OracleFusionHcmMgmt.Implementations.OracleFusion.AtomicHandlers;
using System.Net;
using Microsoft.Extensions.Options;
using OracleFusionHcmMgmt.ConfigModels;

namespace OracleFusionHcmMgmt.Implementations.OracleFusion.Handlers
{
    /// <summary>
    /// Handler for Create Leave operation.
    /// Orchestrates Atomic Handler for Oracle Fusion HCM leave creation.
    /// </summary>
    public class CreateLeaveHandler : IBaseHandler<CreateLeaveReqDTO>
    {
        private readonly ILogger<CreateLeaveHandler> _logger;
        private readonly CreateLeaveAtomicHandler _createLeaveAtomicHandler;
        private readonly AppConfigs _appConfigs;
        
        public CreateLeaveHandler(
            ILogger<CreateLeaveHandler> logger,
            CreateLeaveAtomicHandler createLeaveAtomicHandler,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _createLeaveAtomicHandler = createLeaveAtomicHandler;
            _appConfigs = options.Value;
        }
        
        public async Task<BaseResponseDTO> HandleAsync(CreateLeaveReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Leave in Oracle Fusion HCM");
            
            HttpResponseSnapshot response = await CreateLeaveInDownstream(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"Oracle Fusion HCM API call failed - Status: {response.StatusCode}");
                
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.OFH_LVECRT_0001,
                    errorDetails: [$"Oracle Fusion HCM returned status {response.StatusCode}. Response: {response.Content}"],
                    stepName: "CreateLeaveHandler.cs / HandleAsync"
                );
            }
            else
            {
                CreateLeaveApiResDTO? apiResponse = RestApiHelper.DeserializeJsonResponse<CreateLeaveApiResDTO>(response.Content!);
                
                if (apiResponse == null || apiResponse.PersonAbsenceEntryId == null)
                {
                    throw new NoResponseBodyException(
                        error: ErrorConstants.OFH_LVECRT_0002,
                        errorDetails: ["Oracle Fusion HCM returned empty or invalid response body."],
                        stepName: "CreateLeaveHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    _logger.Info("[System Layer]-Completed Create Leave in Oracle Fusion HCM");
                    
                    return new BaseResponseDTO(
                        message: InfoConstants.CREATE_LEAVE_SUCCESS,
                        data: CreateLeaveResDTO.Map(apiResponse),
                        errorCode: null
                    );
                }
            }
        }
        
        /// <summary>
        /// Calls Atomic Handler to create leave in Oracle Fusion HCM.
        /// Transforms D365 request to Oracle Fusion format.
        /// </summary>
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
                EndDateDuration = request.EndDateDuration,
                Username = _appConfigs.OracleFusionUsername,
                Password = _appConfigs.OracleFusionPassword
            };
            
            return await _createLeaveAtomicHandler.Handle(atomicRequest);
        }
    }
}
