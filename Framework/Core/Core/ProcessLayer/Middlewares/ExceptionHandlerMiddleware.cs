using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.Models;
using Core.ProcessLayer.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace Core.ProcessLayer.Middlewares
{
    public class ExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            BaseResponseDTO? baseResponse = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;

            try
            {
                await next(context);
                baseResponse = context.GetInvocationResult().Value as BaseResponseDTO;
                if (baseResponse != null)
                {
                    if (baseResponse.IsPartialSuccess == true)
                        statusCode = HttpStatusCode.MultiStatus;
                }
                else
                {
                    //This needs to be filled
                }
            }
            catch (Exception ex)
            {

                switch (ex)
                {
                    case RequestValidationFailureException validationEx:
                        _logger.Error(validationEx, $"Handled RequestValidationFailureException with step: {validationEx.StepName}, error: {validationEx.Message}");

                        baseResponse = new BaseResponseDTO(
                            message: validationEx.Message,
                            errorCode: validationEx.ErrorCode,
                            data: null,
                            errorDetails: BuildErrorDetails(validationEx)
                        );
                        statusCode = HttpStatusCode.BadRequest;
                        break;


                    case NoRequestBodyException noNodyEx:
                        _logger.Error(noNodyEx, $"Handled NoRequestBodyException with step: {noNodyEx.StepName}, error: {noNodyEx.Message}");

                        baseResponse = new BaseResponseDTO(
                           message: noNodyEx.Message,
                           errorCode: noNodyEx.ErrorCode,
                           data: null,
                           errorDetails: BuildErrorDetails(noNodyEx)
                        );
                        statusCode = noNodyEx.StatusCode;
                        break;

                    case NoResponseBodyException noResNodyEx:
                        _logger.Error(noResNodyEx, $"Handled NoResponseBodyException with step: {noResNodyEx.StepName}, error: {noResNodyEx.Message}");

                        baseResponse = new BaseResponseDTO(
                           message: noResNodyEx.Message,
                           errorCode: noResNodyEx.ErrorCode,
                           data: null,
                           errorDetails: BuildErrorDetails(noResNodyEx),
                           isDownStreamError: true
                        );
                        statusCode = noResNodyEx.StatusCode;
                        break;

                    case PassThroughHttpException passThroughEx:
                        _logger.Error(passThroughEx,
                            $"Handled PassThroughHttpException with step: {passThroughEx.StepName}, error: {passThroughEx.Message}");

                        baseResponse = new BaseResponseDTO(
                            message: passThroughEx.Message,
                            errorCode: passThroughEx.ErrorCode,
                            data: null,
                            errorDetails: BuildErrorDetails(passThroughEx),
                            isDownStreamError : passThroughEx.IsDownStreamError
                        );

                        statusCode = passThroughEx.StatusCode;
                        break;

                    case HTTPBaseException httpBaseEx:
                        _logger.Error(httpBaseEx, $"Handled HTTPBaseException with step: {httpBaseEx.StepName}, error: {httpBaseEx.Message}");

                        baseResponse = new BaseResponseDTO(
                            message: httpBaseEx.Message,
                            errorCode: httpBaseEx.ErrorCode,
                            data: null,
                            errorDetails: BuildErrorDetails(httpBaseEx)
                        );
                        statusCode = httpBaseEx.StatusCode;
                        break;

                    default:
                        _logger.Error(ex, $"Unhandled Exception with error: {ex.Message}");

                        ErrorDetails errorDetails = new ErrorDetails
                        {
                            Errors = new List<Step>
                            {
                                new Step
                                {
                                    StepName = GetFailingMethodName(ex),
                                    StepError = ex.Message ?? "An unexpected error occurred while processing the request."
                                }
                            }
                        };

                        baseResponse = new BaseResponseDTO(
                            message: "An unexpected error occurred.",
                            errorCode: "UNHANDLED_INTERNAL_ERROR",
                            data: null,
                            errorDetails: errorDetails
                        );
                        statusCode = HttpStatusCode.InternalServerError;
                        break;
                }
            }
            finally
            {
                bool isHttpFunction = context.FunctionDefinition.InputBindings.Any(b => b.Value.Type == "httpTrigger");
                if( isHttpFunction )
                {
                    HttpResponseData? resp = context.Items[typeof(HttpResponseData).Name] as HttpResponseData;
                    if (resp != null)
                    {
                        resp.StatusCode = statusCode;
                    }
                }
                context.GetInvocationResult().Value = baseResponse;
            }
        }

        private static ErrorDetails BuildErrorDetails(HTTPBaseException ex)
        {
            List<Step> errors = ex.ErrorDetails?.Any() == true
                ? ex.ErrorDetails.Select(e => new Step
                {
                    StepName = ex?.StepName!,
                    StepError = e
                }).ToList()
                : new List<Step>
                {
                    new Step
                    {
                        StepName = ex?.StepName!,
                        StepError = ex.Message
                    }
                };

            return new ErrorDetails { Errors = errors };
        }

        private string GetFailingMethodName(Exception ex)
        {
            var stackTrace = new StackTrace(ex, true);
            var frame = stackTrace.GetFrame(0);
            if (frame != null)
            {
                var method = frame.GetMethod();
                if (method != null)
                {
                    if (method?.DeclaringType != null)
                    {
                        string? input = method.DeclaringType.FullName;
                        if (string.IsNullOrEmpty(input))
                            return "Unknown Method";

                        string lastSegment = input.Split('.').Last();

                        var parts = lastSegment.Split('+');
                        string className = parts.Length > 0 ? parts[0] : "UnknownClass";

                        string methodPart = parts.Length > 1 ? parts[1] : string.Empty;

                        var match = Regex.Match(methodPart, @"<(.+?)>");
                        string methodName = match.Success ? match.Groups[1].Value : "UnknownMethod";

                        return $"{className}.cs / {methodName}";
                    }
                }
            }
            return "Unknown Method";
        }
    }
}
