using Core.DTOs;
using Core.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.DTOs.API;
using System.Net;
using System.Text.Json;

namespace SysCafmMgmt.Functions;

/// <summary>
/// Azure Function for Create Work Order endpoint
/// HTTP POST /api/workorders
/// </summary>
public class CreateWorkOrderFunction
{
    private readonly IWorkOrderMgmt _workOrderMgmt;
    private readonly ILogger<CreateWorkOrderFunction> _logger;

    public CreateWorkOrderFunction(
        IWorkOrderMgmt workOrderMgmt,
        ILogger<CreateWorkOrderFunction> logger)
    {
        _workOrderMgmt = workOrderMgmt;
        _logger = logger;
    }

    [Function("CreateWorkOrder")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "workorders")] HttpRequestData req,
        FunctionContext executionContext)
    {
        _logger.Info("CreateWorkOrder function triggered");

        // Store response for middleware
        var response = req.CreateResponse(HttpStatusCode.OK);
        executionContext.Items[typeof(HttpResponseData).Name] = response;

        // Parse request body
        var requestBody = await req.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            throw new Core.Exceptions.NoRequestBodyException(
                "Request body is required",
                "NO_REQUEST_BODY",
                stepName: nameof(CreateWorkOrderFunction));
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        CreateWorkOrderRequestDTO? request;
        try
        {
            request = JsonSerializer.Deserialize<CreateWorkOrderRequestDTO>(requestBody, options);
        }
        catch (JsonException ex)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                $"Invalid JSON format: {ex.Message}",
                "INVALID_JSON",
                stepName: nameof(CreateWorkOrderFunction));
        }

        if (request == null)
        {
            throw new Core.Exceptions.NoRequestBodyException(
                "Failed to parse request body",
                "PARSE_FAILED",
                stepName: nameof(CreateWorkOrderFunction));
        }

        // Call service
        var result = await _workOrderMgmt.CreateWorkOrderAsync(request);
        
        _logger.Info("CreateWorkOrder function completed");
        return result;
    }
}
