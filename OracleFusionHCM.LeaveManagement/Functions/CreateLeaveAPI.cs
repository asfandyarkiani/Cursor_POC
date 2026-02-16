using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OracleFusionHCM.LeaveManagement.Abstractions;
using OracleFusionHCM.LeaveManagement.Constants;
using OracleFusionHCM.LeaveManagement.DTO.CreateLeaveDTO;

namespace OracleFusionHCM.LeaveManagement.Functions
{
    /// <summary>
    /// Azure Function for creating leave entries in Oracle Fusion HCM
    /// HTTP-triggered entry point for System Layer
    /// </summary>
    public class CreateLeaveAPI
    {
        private readonly ILogger<CreateLeaveAPI> _logger;
        private readonly ILeaveMgmt _leaveMgmt;
        
        public CreateLeaveAPI(
            ILogger<CreateLeaveAPI> logger,
            ILeaveMgmt leaveMgmt)
        {
            _logger = logger;
            _leaveMgmt = leaveMgmt;
        }
        
        [Function("CreateLeave")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "leave/create")] HttpRequest req)
        {
            _logger.Info("HTTP trigger received for CreateLeave.");
            
            // Step 1: Read request body
            CreateLeaveReqDTO? request = await req.ReadBodyAsync<CreateLeaveReqDTO>();
            
            // Step 2: Null check (MANDATORY)
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: new List<string> { ErrorConstants.REQ_BODY_MISSING_OR_EMPTY.Message },
                    stepName: "CreateLeaveAPI.cs / Executing Run"
                );
            }
            
            // Step 3: Validate request (MANDATORY)
            request.ValidateAPIRequestParameters();
            
            // Step 4: Delegate to service
            BaseResponseDTO result = await _leaveMgmt.CreateLeave(request);
            
            // Step 5: Return result (middleware handles serialization)
            return result;
        }
    }
}
