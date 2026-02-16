using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using sys_oraclefusion_hcm.Abstractions;
using sys_oraclefusion_hcm.DTO.CreateLeaveDTO;

namespace sys_oraclefusion_hcm.Functions
{
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hcm/leave/create")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Create Leave.");

            CreateLeaveReqDTO? request = await req.ReadBodyAsync<CreateLeaveReqDTO>();

            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: ["Request body is missing or empty"],
                    stepName: "CreateLeaveAPI.cs / Executing Run"
                );
            }

            request.ValidateAPIRequestParameters();

            BaseResponseDTO result = await _leaveMgmt.CreateLeave(request);

            return result;
        }
    }
}
