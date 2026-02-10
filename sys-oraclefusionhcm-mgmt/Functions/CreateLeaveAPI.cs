using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OracleFusionHCMSystem.Abstractions;
using OracleFusionHCMSystem.DTO.CreateLeaveDTO;

namespace OracleFusionHCMSystem.Functions
{
    public class CreateLeaveAPI
    {
        private readonly ILogger<CreateLeaveAPI> _logger;
        private readonly ILeaveMgmt _leaveMgmt;

        public CreateLeaveAPI(ILogger<CreateLeaveAPI> logger, ILeaveMgmt leaveMgmt)
        {
            _logger = logger;
            _leaveMgmt = leaveMgmt;
        }

        [Function("CreateLeave")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "leave/create")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for CreateLeave.");

            CreateLeaveReqDTO? request = await req.ReadBodyAsync<CreateLeaveReqDTO>();

            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [Core.Constants.ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "CreateLeaveAPI.cs / Executing Run"
                );
            }

            request.ValidateAPIRequestParameters();

            BaseResponseDTO result = await _leaveMgmt.CreateLeave(request);

            return result;
        }
    }
}
