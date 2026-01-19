using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.Helper;

namespace FacilitiesMgmtSystem.Middlewares;

/// <summary>
/// Middleware that handles exceptions and normalizes error responses.
/// </summary>
public class ExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (RequestValidationFailureException ex)
        {
            _logger.Warn("Request validation failed: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message, ex.ErrorProperties);
        }
        catch (DownStreamApiFailureException ex)
        {
            _logger.Error(ex, "Downstream API failure: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.BadGateway, ex.Message, ex.ErrorProperties);
        }
        catch (ApiException ex)
        {
            _logger.Error(ex, "API exception: {Message}", ex.Message);
            await WriteErrorResponse(context, (HttpStatusCode)ex.StatusCode, ex.Message, ex.ErrorProperties);
        }
        catch (BaseException ex)
        {
            _logger.Error(ex, "Application exception: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, ex.Message, ex.ErrorProperties);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, 
                "An unexpected error occurred.", Array.Empty<string>());
        }
    }

    private async Task WriteErrorResponse(FunctionContext context, HttpStatusCode statusCode, 
        string message, string[] errorProperties)
    {
        var httpReqData = await context.GetHttpRequestDataAsync();
        if (httpReqData != null)
        {
            var response = httpReqData.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json");

            var errorResponse = new BaseResponseDTO
            {
                Success = false,
                Message = message,
                ErrorProperties = errorProperties
            };

            await response.WriteStringAsync(JsonSerializer.Serialize(errorResponse));
            
            // Set the invocation result
            context.GetInvocationResult().Value = response;
        }
    }
}
