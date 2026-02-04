using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.HandlerDTOs.CreateBreakdownTaskDTO;
using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CAFMSystem.Functions
{
    public class CreateBreakdownTaskAPI
    {
        private readonly ILogger<CreateBreakdownTaskAPI> _logger;
        private readonly ICAFMMgmt _cafmMgmt;

        public CreateBreakdownTaskAPI(ILogger<CreateBreakdownTaskAPI> logger, ICAFMMgmt cafmMgmt)
        {
            _logger = logger;
            _cafmMgmt = cafmMgmt;
        }

        [CAFMAuthentication]
        [Function("CreateBreakdownTask")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/breakdowntask")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Create Breakdown Task.");
            
            CreateBreakdownTaskReqDTO? request = await req.ReadBodyAsync<CreateBreakdownTaskReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "CreateBreakdownTaskAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _cafmMgmt.CreateBreakdownTask(request);
            
            return result;
        }
    }
}
