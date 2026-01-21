using Core.Constants;
using Core.Context;
using Core.Exceptions;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

namespace Core.Extensions
{
    public static class HttpRequestDataExtension
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Reads the HTTP request body and deserializes it into the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to deserialize the request body into.</typeparam>
        /// <param name="httpRequest">The incoming HTTP request containing the body data.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning an instance of type <typeparamref name="T"/> 
        /// if deserialization is successful; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="NoRequestBodyException">
        /// Thrown when the request body is missing, empty, or contains invalid JSON.
        /// </exception>
        public static async Task<T?> ReadBodyAsync<T>(this HttpRequestData httpRequest)
        {
            try
            {
                if (httpRequest == null)
                {
                    List<string> errorDetails = new List<string> { ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message };
                    throw new NoRequestBodyException(null, errorDetails, "HttpRequestDataExtensions / ReadBodyAsync<T>");
                }

                using StreamReader reader = new StreamReader(httpRequest.Body);
                string text = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(text) || text.Trim() == "{}")
                {
                    List<string> errorDetails = new List<string> { ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message };
                    throw new NoRequestBodyException(null, errorDetails, "HttpRequestDataExtensions / ReadBodyAsync<T>");
                }

                Session.RequestBody.Append(text);
                return JsonSerializer.Deserialize<T>(text, _options);
            }
            catch (NoRequestBodyException)
            {
                throw;
            }
        }
    }
}
