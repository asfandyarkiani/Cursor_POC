using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.GetBreakdownTasksByDtoDTO;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CAFMSystem.Functions
{
    /// <summary>
    /// Azure Function for GetBreakdownTasksByDto operation.
    /// Checks if breakdown task already exists in CAFM based on service request number.
    /// </summary>
    public class GetBreakdownTasksByDtoAPI
    {
        private readonly ILogger<GetBreakdownTasksByDtoAPI> _logger;
        private readonly IWorkOrderMgmt _workOrderMgmt;

        public GetBreakdownTasksByDtoAPI(
            ILogger<GetBreakdownTasksByDtoAPI> logger,
            IWorkOrderMgmt workOrderMgmt)
        {
            _logger = logger;
            _workOrderMgmt = workOrderMgmt;
        }

        [CustomAuthentication]
        [Function("GetBreakdownTasksByDto")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/breakdown-tasks/check")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for GetBreakdownTasksByDto.");

            // Read request body
            GetBreakdownTasksByDtoReqDTO? request = await req.ReadBodyAsync<GetBreakdownTasksByDtoReqDTO>();

            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: new System.Collections.Generic.List<string> { Core.Constants.ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                    stepName: "GetBreakdownTasksByDtoAPI.cs / Executing Run"
                );
            }
            else
            {
                // Validate request
                request.ValidateAPIRequestParameters();

                // Delegate to service
                BaseResponseDTO result = await _workOrderMgmt.GetBreakdownTasksByDto(request);

                return result;
            }
        }
    }
}
