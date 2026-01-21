using Core.Constants;
using Core.Context;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.Extensions
{
    public static class HttpRequestExtensions
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<string> ReadBodyAsync(this HttpRequest httpRequest)
        {
            if (httpRequest == null)
                throw new ArgumentNullException(nameof(httpRequest));

            using StreamReader reader = new StreamReader(httpRequest.Body);
            String body = await reader.ReadToEndAsync();

            Session.RequestBody.Append(body);
            
            return body;
        }

        public static async Task<T?> ReadBodyAsync<T>(this HttpRequest httpRequest)
        {
            try
            {
                if (httpRequest == null)
                {
                    throw new NoRequestBodyException(
                        errorDetails: new List<string> { ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                        stepName: "HttpRequestExtensions / ReadBodyAsync<T>"
                    );
                }
                using StreamReader reader = new StreamReader(httpRequest.Body);
                string text = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(text) || text.Trim() == "{}")
                {
                    throw new NoRequestBodyException(
                        errorDetails: new List<string> { ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                        stepName: "HttpRequestExtensions / ReadBodyAsync<T>"
                    );
                }
                Session.RequestBody.Append(text);
                return JsonSerializer.Deserialize<T>(text, _options);
            }
            catch (NoRequestBodyException)
            {
                throw;
            }
        }

        public static async Task<T?> ReadBodyAndQueryAsync<T>(this HttpRequest httpRequest) where T : class, new()
        {
            try
            {
                T? dto = new();
                if (httpRequest == null)
                {
                    throw new NoRequestBodyException(
                        errorDetails: new List<string> { ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                        stepName: "HttpRequestExtensions / ReadBodyAndQueryAsync"
                    );
                }

                // 1) Deserialize body if exists
                if (httpRequest.Body != null && httpRequest.ContentLength > 0)
                {
                    using StreamReader reader = new StreamReader(httpRequest.Body);
                    string text = await reader.ReadToEndAsync();
                    if (string.IsNullOrWhiteSpace(text) || text.Trim() == "{}")
                    {
                        throw new NoRequestBodyException(
                            errorDetails: new List<string> { ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                            stepName: "HttpRequestExtensions / ReadBodyAndQueryAsync"
                        );
                    }

                    Session.RequestBody.Append(text);
                    dto = JsonSerializer.Deserialize<T>(text, _options);
                }

                // 2) Override with query parameters
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (httpRequest.Query.TryGetValue(prop.Name, out var value) && !string.IsNullOrEmpty(value))
                    {
                        var convertedValue = Convert.ChangeType(value.ToString(), Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        prop.SetValue(dto, convertedValue);
                    }
                }
                return dto;
            }
            catch (NoRequestBodyException)
            {
                throw;
            }
        }
    }
}