using CAFMManagementSystem.Abstractions;
using CAFMManagementSystem.Attributes;
using CAFMManagementSystem.DTO.GetBreakdownTasksByDtoDTO;
using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CAFMManagementSystem.Functions
{
    public class GetBreakdownTasksByDtoAPI
    {
        private readonly ILogger<GetBreakdownTasksByDtoAPI> _logger;
        private readonly IBreakdownTaskMgmt _breakdownTaskMgmt;
        
        public GetBreakdownTasksByDtoAPI(
            ILogger<GetBreakdownTasksByDtoAPI> logger,
            IBreakdownTaskMgmt breakdownTaskMgmt)
        {
            _logger = logger;
            _breakdownTaskMgmt = breakdownTaskMgmt;
        }
        
        [CustomAuthentication]
        [Function("GetBreakdownTasksByDto")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/breakdown-tasks/query")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Get Breakdown Tasks By Dto.");
            
            GetBreakdownTasksByDtoReqDTO? request = await req.ReadBodyAsync<GetBreakdownTasksByDtoReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "GetBreakdownTasksByDtoAPI.cs / Executing Run"
                );
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _breakdownTaskMgmt.GetBreakdownTasksByDto(request);
            
            return result;
        }
    }
}
