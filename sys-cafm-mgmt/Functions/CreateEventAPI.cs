using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Abstractions;
using sys_cafm_mgmt.Attributes;
using sys_cafm_mgmt.DTO.CreateEventDTO;

namespace sys_cafm_mgmt.Functions
{
    public class CreateEventAPI
    {
        private readonly ILogger<CreateEventAPI> _logger;
        private readonly IWorkOrderMgmt _workOrderMgmt;

        public CreateEventAPI(
            ILogger<CreateEventAPI> logger,
            IWorkOrderMgmt workOrderMgmt)
        {
            _logger = logger;
            _workOrderMgmt = workOrderMgmt;
        }

        [CustomAuthentication]
        [Function("CreateEvent")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/event")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for CreateEvent.");
            
            CreateEventReqDTO? request = await req.ReadBodyAsync<CreateEventReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [Core.Constants.ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "CreateEventAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _workOrderMgmt.CreateEvent(request);
            
            return result;
        }
    }
}
