using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Core.Headers;
using System.Text.Json;
using Core.Context;
using Core.DTOs;
using Core.Extensions;

namespace Core.ProcessLayer.Middlewares
{
    public class ExecutionTimingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ExecutionTimingMiddleware> _log;
        private static readonly string[] ReqHeadersToBeAdded = { RequestHeaders.TestRunId.Item1, RequestHeaders.RequestId.Item1 };
        private static readonly string[] ResHeadersToBeAdded = { ResponseHeaders.SYSTotalTime.Item1, ResponseHeaders.DSTimeBreakDown.Item1, ResponseHeaders.DSAggregatedTime.Item1, ResponseHeaders.ContentTypeJson.Item1, ResponseHeaders.IsDownStreamError.Item1 };

        private const string REQUEST_BODY = "Request-Body";
        private const string RESPONSE_BODY = "Response-Body";

        public ExecutionTimingMiddleware(ILogger<ExecutionTimingMiddleware> log) => _log = log;

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            Stopwatch swtotal = Stopwatch.StartNew();

            RequestHeaders.TestRunId.Item2.Value = new StringBuilder();
            RequestHeaders.RequestId.Item2.Value = new StringBuilder();
            ResponseHeaders.PROCTotalTime.Item2.Value = new StringBuilder();
            ResponseHeaders.DSTimeBreakDown.Item2.Value = new StringBuilder();
            ResponseHeaders.SYSTotalTime.Item2.Value = new StringBuilder();
            ResponseHeaders.IsDownStreamError.Item2.Value = new StringBuilder();
            Session.RequestBody = new StringBuilder();

            HttpRequestData? req = null;
            HttpResponseData? resp = null;
            Activity? activity = null;

            bool isHttpFunction = context.FunctionDefinition.InputBindings.Any(b => b.Value.Type == "httpTrigger");

            if (isHttpFunction)
            {
                req = context.GetHttpRequestDataAsync().GetAwaiter().GetResult();
                resp = req.CreateResponse();
                context.Items.Add(typeof(HttpResponseData).Name, resp);

                activity = Activity.Current;

                if (activity != null && req != null)
                {
                    HttpHeadersCollection receivedRequestHeaders = req.Headers;
                    foreach (String headerToBeAdded in ReqHeadersToBeAdded)
                    {
                        if (!receivedRequestHeaders.TryGetValues(headerToBeAdded, out IEnumerable<String>? headerValues))
                            continue;

                        String? headerValue = headerValues.FirstOrDefault();

                        if (String.IsNullOrWhiteSpace(headerValue))
                            continue;

                        if (headerToBeAdded.Equals(RequestHeaders.TestRunId.Item1, StringComparison.OrdinalIgnoreCase))
                        {
                            RequestHeaders.TestRunId.Item2.Value?.Append(headerValue);
                        }
                        else if (headerToBeAdded.Equals(RequestHeaders.RequestId.Item1, StringComparison.OrdinalIgnoreCase))
                        {
                            RequestHeaders.RequestId.Item2.Value?.Append(headerValue);
                        }

                        activity.SetTag(headerToBeAdded, headerValue);
                        _log.Info($"request header {headerToBeAdded}={headerValue}");
                    }
                }
            }
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unahandled exception occured in Execution timing middleware.");
            }
            finally
            {
                activity ??= Activity.Current;

                BaseResponseDTO? baseResponse = context.GetInvocationResult().Value as BaseResponseDTO;

                foreach (string header in ResHeadersToBeAdded)
                {
                    string? headerValue = header switch
                    {
                        var h when h.Equals(ResponseHeaders.SYSTotalTime.Item1) => ResponseHeaders.SYSTotalTime.Item2.Value?.ToString(),
                        var h when h.Equals(ResponseHeaders.DSTimeBreakDown.Item1) => ResponseHeaders.DSTimeBreakDown.Item2.Value?.ToString(),
                        var h when h.Equals(ResponseHeaders.DSAggregatedTime.Item1) => ResponseHeaders.DSAggregatedTime.Item2.ToString(),
                        var h when h.Equals(ResponseHeaders.ContentTypeJson.Item1) => ResponseHeaders.ContentTypeJson.Item2,
                        var h when h.Equals(ResponseHeaders.IsDownStreamError.Item1) => ResponseHeaders.IsDownStreamError.Item2.Value?.ToString(),
                        _ => null
                    };

                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        activity?.SetTag(header, headerValue);
                        _log.Info($"Response header {header}={headerValue}");

                        if (isHttpFunction)
                        {
                            if (resp.Headers.Contains(header))
                                resp.Headers.Remove(header);

                            resp.Headers.Add(header, headerValue);
                        }
                    }
                }

                String requestBody = Session.RequestBody.ToString();
                activity.SetTag(REQUEST_BODY, requestBody);

                String responseBody = JsonSerializer.Serialize(baseResponse);
                activity.SetTag(RESPONSE_BODY, responseBody);

                swtotal.Stop();
                ResponseHeaders.PROCTotalTime.Item2.Value?.Append($"{swtotal.ElapsedMilliseconds}");

                //setting activity tag
                activity.SetTag(ResponseHeaders.PROCTotalTime.Item1, ResponseHeaders.PROCTotalTime.Item2.Value);
                _log.Info($"Response header {ResponseHeaders.PROCTotalTime.Item1}={ResponseHeaders.PROCTotalTime.Item2.Value}");

                if (isHttpFunction)
                {
                    //adding process total time in response headers
                    resp.Headers.Add(ResponseHeaders.PROCTotalTime.Item1, ResponseHeaders.PROCTotalTime.Item2.Value?.ToString());

                    await resp.WriteStringAsync(JsonSerializer.Serialize(baseResponse));
                    context.GetInvocationResult().Value = resp;
                }
            }
        }
    }
}