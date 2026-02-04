using Core.DTOs;
using Core.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.DTOs.Requests;
using System.Net;
using System.Text.Json;

namespace SysCafmMgmt.Functions
{
    /// <summary>
    /// Azure Function for Create Work Order operation
    /// HTTP endpoint for EQ+ to CAFM work order creation
    /// </summary>
    public class CreateWorkOrderFunction
    {
        private readonly IWorkOrderMgmt _workOrderService;
        private readonly ILogger<CreateWorkOrderFunction> _logger;

        public CreateWorkOrderFunction(
            IWorkOrderMgmt workOrderService,
            ILogger<CreateWorkOrderFunction> logger)
        {
            _workOrderService = workOrderService;
            _logger = logger;
        }

        /// <summary>
        /// HTTP trigger for creating work orders in CAFM
        /// </summary>
        [Function("CreateWorkOrder")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "workorders")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.Info("CreateWorkOrder function triggered");

            // Store HttpResponseData for middleware to set status code
            var response = req.CreateResponse();
            executionContext.Items[typeof(HttpResponseData).Name] = response;

            // Parse request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                throw new Core.Exceptions.NoRequestBodyException(
                    message: "Request body is required",
                    errorCode: "NO_REQUEST_BODY",
                    statusCode: HttpStatusCode.BadRequest,
                    stepName: "CreateWorkOrderFunction.Run"
                );
            }

            CreateWorkOrderRequestDto? requestDto;
            try
            {
                requestDto = JsonSerializer.Deserialize<CreateWorkOrderRequestDto>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.Error(ex, $"Failed to deserialize request body: {ex.Message}");
                throw new Core.Exceptions.RequestValidationFailureException(
                    message: $"Invalid JSON format: {ex.Message}",
                    errorCode: "INVALID_JSON",
                    statusCode: HttpStatusCode.BadRequest,
                    stepName: "CreateWorkOrderFunction.Run"
                );
            }

            if (requestDto == null)
            {
                throw new Core.Exceptions.NoRequestBodyException(
                    message: "Failed to parse request body",
                    errorCode: "INVALID_REQUEST",
                    statusCode: HttpStatusCode.BadRequest,
                    stepName: "CreateWorkOrderFunction.Run"
                );
            }

            _logger.Info($"Processing work order creation request");

            var result = await _workOrderService.CreateWorkOrderAsync(requestDto);

            return result;
        }
    }
}
