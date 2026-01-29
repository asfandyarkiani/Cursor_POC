using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.CreateEventDTO;
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
    /// Azure Function for CreateEvent operation.
    /// Links a recurring event to an existing breakdown task in CAFM.
    /// </summary>
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/events/create")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for CreateEvent.");

            // Read request body
            CreateEventReqDTO? request = await req.ReadBodyAsync<CreateEventReqDTO>();

            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: new System.Collections.Generic.List<string> { Core.Constants.ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                    stepName: "CreateEventAPI.cs / Executing Run"
                );
            }
            else
            {
                // Validate request
                request.ValidateAPIRequestParameters();

                // Delegate to service
                BaseResponseDTO result = await _workOrderMgmt.CreateEvent(request);

                return result;
            }
        }
    }
}
