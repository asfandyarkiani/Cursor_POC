using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using AGI.SystemLayer.CAFM.Abstractions;
using AGI.SystemLayer.CAFM.Attributes;
using AGI.SystemLayer.CAFM.DTOs.API;
using Core.Extensions;

namespace AGI.SystemLayer.CAFM.Functions;

/// <summary>
/// Azure Function for creating work orders in CAFM from external systems (e.g., EQ+).
/// This is the HTTP entry point for the System Layer.
/// </summary>
public class CreateWorkOrderFunction
{
    private readonly ICAFMMgmt _cafmService;
    private readonly ILogger<CreateWorkOrderFunction> _logger;

    public CreateWorkOrderFunction(
        ICAFMMgmt cafmService,
        ILogger<CreateWorkOrderFunction> logger)
    {
        _cafmService = cafmService;
        _logger = logger;
    }

    /// <summary>
    /// HTTP POST endpoint to create work orders in CAFM.
    /// Endpoint: POST /api/cafm/workorders
    /// </summary>
    /// <param name="req">HTTP request containing CreateWorkOrderRequestDTO</param>
    /// <param name="executionContext">Function execution context</param>
    /// <returns>CreateWorkOrderResponseDTO with CAFM-generated task IDs</returns>
    [Function("CreateWorkOrder")]
    [CAFMAuthentication(AutoLogout = true)]
    public async Task<HttpResponseData> CreateWorkOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/workorders")] HttpRequestData req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("CreateWorkOrder: Processing request");

        try
        {
            // Read and deserialize request body
            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("CreateWorkOrder: Empty request body");
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, new CreateWorkOrderResponseDTO
                {
                    Success = false,
                    Message = "Request body is required"
                });
            }

            var request = JsonSerializer.Deserialize<CreateWorkOrderRequestDTO>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
            {
                _logger.LogWarning("CreateWorkOrder: Failed to deserialize request");
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, new CreateWorkOrderResponseDTO
                {
                    Success = false,
                    Message = "Invalid request format"
                });
            }

            // Validate request
            if (request.WorkOrder?.ServiceRequests == null || !request.WorkOrder.ServiceRequests.Any())
            {
                _logger.LogWarning("CreateWorkOrder: No service requests in request");
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, new CreateWorkOrderResponseDTO
                {
                    Success = false,
                    Message = "At least one service request is required"
                });
            }

            // Process the request
            var response = await _cafmService.CreateWorkOrderAsync(request, executionContext.CancellationToken);

            // Return appropriate HTTP status code
            var statusCode = response.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

            _logger.LogInformation("CreateWorkOrder: Request processed. Success: {Success}, Message: {Message}",
                response.Success, response.Message);

            return await req.CreateResponseAsync(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkOrder: Unhandled exception");

            return await req.CreateResponseAsync(HttpStatusCode.InternalServerError, new CreateWorkOrderResponseDTO
            {
                Success = false,
                Message = $"An error occurred: {ex.Message}"
            });
        }
    }
}
