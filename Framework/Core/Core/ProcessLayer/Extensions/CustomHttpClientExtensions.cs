using Core.Extensions;
using Core.Headers;
using Core.Middlewares;
using System.Text;
using System.Text.Json;


namespace Core.ProcessLayer.Extensions
{
    public static class CustomHttpClientExtensions
    {
        public static Task<HttpResponseMessage> SendProcessHTTPReqAsync(
            this CustomHTTPClient client,
            HttpMethod method,
            string url,
            string contentType = "application/json",
            object? body = null,
            List<Tuple<string, string>>? reqHeaders = null
        )
        {
            reqHeaders ??= new List<Tuple<string, string>>();

            reqHeaders.Add(RequestHeaders.TestRunId.Simplify());
            reqHeaders.Add(RequestHeaders.RequestId.Simplify());

            string? json = body is not null
                ? JsonSerializer.Serialize(body)
                : null;

            Task<HttpResponseMessage> response = client.SendAsync(
                method,
                url,
                () => json is null ? null : new StringContent(json, Encoding.UTF8, contentType),
                reqHeaders ?? new List<Tuple<string, string>>()
            );


            if (ResponseHeaders.AllSysHeaders?.Count > 0)
            {
                foreach (var header in ResponseHeaders.AllSysHeaders)
                {
                    ResponseHeaders.AllProcHeaders.Add(
                        new Tuple<string, string>(header.Item1, header.Item2)
                    );
                }
            }

            return response;
        }
    }
}
