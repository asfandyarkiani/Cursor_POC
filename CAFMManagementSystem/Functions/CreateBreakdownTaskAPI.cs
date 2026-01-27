using CAFMManagementSystem.Abstractions;
using CAFMManagementSystem.Attributes;
using CAFMManagementSystem.DTO.CreateBreakdownTaskDTO;
using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CAFMManagementSystem.Functions
{
    public class CreateBreakdownTaskAPI
    {
        private readonly ILogger<CreateBreakdownTaskAPI> _logger;
        private readonly IBreakdownTaskMgmt _breakdownTaskMgmt;
        
        public CreateBreakdownTaskAPI(
            ILogger<CreateBreakdownTaskAPI> logger,
            IBreakdownTaskMgmt breakdownTaskMgmt)
        {
            _logger = logger;
            _breakdownTaskMgmt = breakdownTaskMgmt;
        }
        
        [CustomAuthentication]
        [Function("CreateBreakdownTask")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/breakdown-tasks/create")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Create Breakdown Task.");
            
            CreateBreakdownTaskReqDTO? request = await req.ReadBodyAsync<CreateBreakdownTaskReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "CreateBreakdownTaskAPI.cs / Executing Run"
                );
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _breakdownTaskMgmt.CreateBreakdownTask(request);
            
            return result;
        }
    }
}
