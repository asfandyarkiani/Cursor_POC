using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.Helpers;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using OracleFusionHCM.LeaveManagement.Constants;
using OracleFusionHCM.LeaveManagement.DTO.AtomicHandlerDTOs;
using OracleFusionHCM.LeaveManagement.DTO.CreateLeaveDTO;
using OracleFusionHCM.LeaveManagement.DTO.DownstreamDTOs;
using OracleFusionHCM.LeaveManagement.Implementations.OracleFusion.AtomicHandlers;
using System.Net;

namespace OracleFusionHCM.LeaveManagement.Implementations.OracleFusion.Handlers
{
    /// <summary>
    /// Handler for Create Leave operation
    /// Orchestrates CreateLeaveAtomicHandler and handles response transformation
    /// </summary>
    public class CreateLeaveHandler : IBaseHandler<CreateLeaveReqDTO>
    {
        private readonly ILogger<CreateLeaveHandler> _logger;
        private readonly CreateLeaveAtomicHandler _createLeaveAtomicHandler;
        private readonly string _username;
        private readonly string _password;
        
        public CreateLeaveHandler(
            ILogger<CreateLeaveHandler> logger,
            CreateLeaveAtomicHandler createLeaveAtomicHandler,
            Microsoft.Extensions.Options.IOptions<ConfigModels.AppConfigs> options)
        {
            _logger = logger;
            _createLeaveAtomicHandler = createLeaveAtomicHandler;
            _username = options.Value.OracleFusionUsername;
            _password = options.Value.OracleFusionPassword;
        }
        
        public async Task<BaseResponseDTO> HandleAsync(CreateLeaveReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Leave");
            
            Core.SystemLayer.Middlewares.HttpResponseSnapshot response = await CreateLeaveInDownstream(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"Oracle Fusion HCM API failed: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.OFC_LEVCRT_0001,
                    errorDetails: new List<string> { $"Response: {response.Content}" },
                    stepName: "CreateLeaveHandler.cs / HandleAsync"
                );
            }
            else
            {
                CreateLeaveApiResDTO? apiResponse = RestApiHelper.DeserializeJsonResponse<CreateLeaveApiResDTO>(response.Content!);
                
                if (apiResponse == null || apiResponse.PersonAbsenceEntryId == null)
                {
                    throw new Core.SystemLayer.Exceptions.NoResponseBodyException(
                        error: ErrorConstants.OFC_LEVCRT_0002,
                        errorDetails: new List<string> { "Oracle Fusion HCM returned empty or invalid response" },
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
        
        /// <summary>
        /// Private method to call Atomic Handler with transformed request
        /// Transforms D365 format to Oracle Fusion format
        /// </summary>
        private async Task<Core.SystemLayer.Middlewares.HttpResponseSnapshot> CreateLeaveInDownstream(CreateLeaveReqDTO request)
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
                Username = _username,
                Password = _password
            };
            
            return await _createLeaveAtomicHandler.Handle(atomicRequest);
        }
    }
}
