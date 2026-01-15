using System.Net;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.Core.System.Exceptions;
using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.Helper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace FacilitiesMgmtSystem.Middlewares;

/// <summary>
/// Middleware to handle exceptions and normalize error responses.
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
            _logger.Warn("Validation failure: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, 
                ex.Message ?? ErrorConstants.INVALID_REQ_PAYLOAD, ex.ErrorProperties);
        }
        catch (DownStreamApiFailureException ex)
        {
            _logger.Error(ex, "Downstream API failure: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadGateway, 
                ex.Message ?? ErrorConstants.DOWNSTREAM_API_FAILURE, ex.ErrorProperties);
        }
        catch (ApiException ex)
        {
            _logger.Error(ex, "API exception: {Message}", ex.Message);
            var statusCode = ex.StatusCode.HasValue 
                ? (HttpStatusCode)ex.StatusCode.Value 
                : HttpStatusCode.InternalServerError;
            await HandleExceptionAsync(context, statusCode, ex.Message, ex.ErrorProperties);
        }
        catch (BaseException ex)
        {
            _logger.Error(ex, "Application exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, 
                ex.Message, ex.ErrorProperties);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, 
                ErrorConstants.INTERNAL_SERVER_ERROR, null);
        }
    }

    private static async Task HandleExceptionAsync(
        FunctionContext context, 
        HttpStatusCode statusCode, 
        string? message, 
        string[]? errors)
    {
        var request = await context.GetHttpRequestDataAsync();
        if (request != null)
        {
            var response = request.CreateResponse(statusCode);
            var errorResponse = BaseResponseDTO.CreateFailure(message, errors);
            await response.WriteAsJsonAsync(errorResponse);
            context.GetInvocationResult().Value = response;
        }
    }
}
