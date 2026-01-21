using Core.Context;
using Core.DTOs;
using Core.Extensions;
using Core.Headers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Core.SystemLayer.Middlewares
{
    public class ExecutionTimingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ExecutionTimingMiddleware> _log;
        private static readonly string[] ReqHeadersToBeAdded = { RequestHeaders.TestRunId.Item1, RequestHeaders.RequestId.Item1 };
        private static readonly string[] ResHeadersToBeAdded = { ResponseHeaders.DSTimeBreakDown.Item1, ResponseHeaders.DSAggregatedTime.Item1, ResponseHeaders.ContentTypeJson.Item1, ResponseHeaders.IsDownStreamError.Item1 };
       
        private const string REQUEST_BODY = "Request-Body";
        private const string RESPONSE_BODY = "Response-Body";
        public ExecutionTimingMiddleware(ILogger<ExecutionTimingMiddleware> log) => _log = log;

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            Stopwatch swtotal = Stopwatch.StartNew();

            RequestHeaders.TestRunId.Item2.Value = new StringBuilder();
            RequestHeaders.RequestId.Item2.Value = new StringBuilder();
            ResponseHeaders.SYSTotalTime.Item2.Value = new StringBuilder();
            ResponseHeaders.DSTimeBreakDown.Item2.Value = new StringBuilder();
            ResponseHeaders.IsDownStreamError.Item2.Value = new StringBuilder();
            ResponseHeaders.IsDownloadOrPreview.Item2.Value = new StringBuilder();
            ResponseHeaders.AllSysHeaders = new List<Tuple<string, string>>();
            Session.RequestBody = new StringBuilder();

            HttpRequestData? req = context.GetHttpRequestDataAsync().GetAwaiter().GetResult();
            HttpResponseData resp = req.CreateResponse();
            context.Items.Add(typeof(HttpResponseData).Name, resp);

            Activity? act = Activity.Current;
            // Capture request headers
            if (act != null && req != null)
            {
                HttpHeadersCollection receivedRequestHeaders = req.Headers;
                foreach (string headerToBeAdded in ReqHeadersToBeAdded)
                {
                    if (!receivedRequestHeaders.TryGetValues(headerToBeAdded, out IEnumerable<String>? headerValues))
                        continue;

                    string? headerValue = headerValues.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(headerValue))
                        continue;

                    if (headerToBeAdded.Equals(RequestHeaders.TestRunId.Item1, StringComparison.OrdinalIgnoreCase))
                    {
                        RequestHeaders.TestRunId.Item2.Value?.Append(headerValue);
                    }
                    else if (headerToBeAdded.Equals(RequestHeaders.RequestId.Item1, StringComparison.OrdinalIgnoreCase))
                    {
                        RequestHeaders.RequestId.Item2.Value?.Append(headerValue);
                    }

                    act.SetTag(headerToBeAdded, headerValue);
                    _log.Info($"request header {headerToBeAdded}={headerValue}");
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
                BaseResponseDTO? baseResponse = context.GetInvocationResult().Value as BaseResponseDTO;
                string? downStreamTime = ResponseHeaders.DSTimeBreakDown.Item2.Value.ToString();
                if (downStreamTime.EndsWith(","))
                {
                    downStreamTime = downStreamTime[..^1];
                }

                int aggregatedDownStreamTime = ResponseHeaders.DSAggregatedTime.Item2;
                resp.Headers.Add(ResponseHeaders.ContentTypeJson.Item1, ResponseHeaders.ContentTypeJson.Item2);
                resp.Headers.Add(ResponseHeaders.DSAggregatedTime.Item1, ResponseHeaders.DSAggregatedTime.Item2.ToString());
                resp.Headers.Add(ResponseHeaders.DSTimeBreakDown.Item1, downStreamTime);
                resp.Headers.Add(ResponseHeaders.IsDownStreamError.Item1, baseResponse.IsDownStreamError.ToString());

                if (act != null && resp != null)
                {
                    HttpHeadersCollection headers = resp.Headers;
                    foreach (String header in ResHeadersToBeAdded)
                    {
                        if (headers.TryGetValues(header, out IEnumerable<String>? headerValues))
                        {
                            String? headerValue = headerValues.FirstOrDefault();
                            if (!String.IsNullOrEmpty(headerValue))
                            {
                                act.SetTag(header, headerValue);
                                _log.Info($"response header {header}={headerValue}");
                            }
                        }
                    }

                    String requestBody = Session.RequestBody.ToString();
                    act.SetTag(REQUEST_BODY, requestBody);

                    String responseBody = JsonSerializer.Serialize(baseResponse);
                    act.SetTag(RESPONSE_BODY, responseBody);
                }


                swtotal.Stop();
                ResponseHeaders.SYSTotalTime.Item2.Value?.Append($"{swtotal.ElapsedMilliseconds}");

                //setting activity tag
                act.SetTag(ResponseHeaders.SYSTotalTime.Item1, ResponseHeaders.SYSTotalTime.Item2.Value);

                //adding system total time in response headers
                resp.Headers.Add(ResponseHeaders.SYSTotalTime.Item1, ResponseHeaders.SYSTotalTime.Item2.Value?.ToString());


                bool isDownloadOrPreview = ResponseHeaders.IsDownloadOrPreview.Item2.Value != null &&
                          ResponseHeaders.IsDownloadOrPreview.Item2.Value.ToString().Equals("True", StringComparison.OrdinalIgnoreCase);

                if (ResponseHeaders.AllSysHeaders != null && ResponseHeaders.AllSysHeaders.Count > 0)
                {
                    foreach (var (key, value) in ResponseHeaders.AllSysHeaders)
                    {
                        // Remove existing header BEFORE adding new one
                        if (resp.Headers.Contains(key))
                            resp.Headers.Remove(key);

                        resp.Headers.TryAddWithoutValidation(key, value);
                    }
                }

                if (isDownloadOrPreview )
                {
                    await resp.Body.WriteAsync((byte[])baseResponse.Data);
                }
                else
                {
                    await resp.WriteStringAsync(JsonSerializer.Serialize(baseResponse));
                }
                   
                context.GetInvocationResult().Value = resp;
            }
        }
    }
}
