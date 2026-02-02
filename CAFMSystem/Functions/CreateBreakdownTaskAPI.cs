using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.CreateBreakdownTaskDTO;
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
    /// Azure Function for CreateBreakdownTask operation.
    /// Creates a new breakdown task/work order in CAFM system.
    /// Orchestrates lookups (locations, instructions) and creates task.
    /// </summary>
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/breakdown-tasks/create")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for CreateBreakdownTask.");

            // Read request body
            CreateBreakdownTaskReqDTO? request = await req.ReadBodyAsync<CreateBreakdownTaskReqDTO>();

            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: new System.Collections.Generic.List<string> { Core.Constants.ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                    stepName: "CreateBreakdownTaskAPI.cs / Executing Run"
                );
            }
            else
            {
                // Validate request
                request.ValidateAPIRequestParameters();

                // Delegate to service
                BaseResponseDTO result = await _workOrderMgmt.CreateBreakdownTask(request);

                return result;
            }
        }
    }
}
