using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.HandlerDTOs.CreateWorkOrderDTO;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CAFMSystem.Functions
{
    /// <summary>
    /// Azure Function for creating work orders in CAFM system.
    /// Entry point for Process Layer to create work orders from EQ+ to CAFM.
    /// Requires CAFM session-based authentication (handled by middleware).
    /// </summary>
    public class CreateWorkOrderAPI
    {
        private readonly ILogger<CreateWorkOrderAPI> _logger;
        private readonly IWorkOrderMgmt _workOrderMgmt;

        public CreateWorkOrderAPI(
            ILogger<CreateWorkOrderAPI> logger,
            IWorkOrderMgmt workOrderMgmt)
        {
            _logger = logger;
            _workOrderMgmt = workOrderMgmt;
        }

        [CustomAuthentication]
        [Function("CreateWorkOrder")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workorder/create")] HttpRequest req)
        {
            _logger.Info("HTTP trigger received for CreateWorkOrder.");

            CreateWorkOrderReqDTO? request = await req.ReadBodyAsync<CreateWorkOrderReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: new List<string> { "Request body is missing or empty" },
                    stepName: "CreateWorkOrderAPI.cs / Executing Run");
            }

            request.ValidateAPIRequestParameters();

            BaseResponseDTO result = await _workOrderMgmt.CreateWorkOrder(request);

            return result;
        }
    }
}
