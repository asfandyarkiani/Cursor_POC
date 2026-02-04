using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Abstractions;
using sys_cafm_mgmt.Attributes;
using sys_cafm_mgmt.DTO.CreateBreakdownTaskDTO;

namespace sys_cafm_mgmt.Functions
{
    public class CreateBreakdownTaskAPI
    {
        private readonly ILogger<CreateBreakdownTaskAPI> _logger;
        private readonly IWorkOrderMgmt _workOrderMgmt;

        public CreateBreakdownTaskAPI(
            ILogger<CreateBreakdownTaskAPI> logger,
            IWorkOrderMgmt workOrderMgmt)
        {
            _logger = logger;
            _workOrderMgmt = workOrderMgmt;
        }

        [CustomAuthentication]
        [Function("CreateBreakdownTask")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/breakdown-task")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for CreateBreakdownTask.");
            
            CreateBreakdownTaskReqDTO? request = await req.ReadBodyAsync<CreateBreakdownTaskReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [Core.Constants.ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "CreateBreakdownTaskAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _workOrderMgmt.CreateBreakdownTask(request);
            
            return result;
        }
    }
}
