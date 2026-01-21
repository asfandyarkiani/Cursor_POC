using Core.Constants;
using Core.Exceptions;
using Core.Extensions;
using Core.Headers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Core.Middlewares
{
    public class CustomRestClient
    {
        private readonly CustomHTTPClient _customHTTPClient;
        private readonly ILogger<CustomRestClient> _logger;

        public CustomRestClient(CustomHTTPClient customHTTPClient, ILogger<CustomRestClient> logger)
        {
            _customHTTPClient = customHTTPClient;
            _logger = logger;
        }

        /// <summary>
        /// Creates a StringContent object for REST API requests with JSON content type
        /// </summary>
        /// <param name="jsonContent">The JSON content as string</param>
        /// <param name="encoding">The encoding to use (default: UTF8)</param>
        /// <returns>StringContent configured for JSON requests</returns>
        public static StringContent CreateJsonContent(string jsonContent, Encoding? encoding = null, string mediaType = "application/json")
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return new StringContent(jsonContent, encoding, mediaType);
        }

        /// <summary>
        /// Creates a StringContent object for REST API requests with JSON content type from an object
        /// </summary>
        /// <param name="data">The object to serialize to JSON</param>
        /// <param name="options">JSON serializer options (optional)</param>
        /// <param name="encoding">The encoding to use (default: UTF8)</param>
        /// <returns>StringContent configured for JSON requests</returns>
        public static StringContent CreateJsonContent<T>(T data, JsonSerializerOptions? options = null, Encoding? encoding = null, string mediaType = "application/json")
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return new StringContent(JsonSerializer.Serialize(data, options), encoding, mediaType);
        }

        /// <summary>
        /// Creates FormUrlEncodedContent with custom headers for REST API requests.
        /// </summary>
        /// <param name="formData">Dictionary of key-value pairs to encode</param>
        /// <param name="headers">Optional custom headers</param>
        /// <returns>HttpContent with form-encoded data and headers</returns>
        public static HttpContent CreateFormUrlEncodedContent(Dictionary<string, string> formData)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(formData ?? new Dictionary<string, string>());

            return content;
        }

        /// <summary>
        /// Creates HttpContent with custom headers for REST API requests
        /// </summary>
        /// <param name="content">The content to send</param>
        /// <param name="headers">Dictionary of custom headers to add</param>
        /// <returns>HttpContent with custom headers applied</returns>
        public static HttpContent CreateContentWithHeaders(HttpContent content, Dictionary<string, string>? headers = null)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    content.Headers.Add(header.Key, header.Value);
                }
            }
            return content;
        }

        /// <summary>
        /// Executes a REST API request using the configured CustomHTTPClient.
        /// Throws RequestValidationFailureException if the URL is invalid or not provided.
        /// </summary>
        /// <param name="operationName">Name of the REST operation (used for logging)</param>
        /// <param name="apiUrl">The endpoint URL of the REST API</param>
        /// <param name="httpMethod">The HTTP method (GET, POST, PUT, DELETE, etc.)</param>
        /// <param name="contentFactory">Factory function to create the HTTP content (optional for GET requests)</param>
        /// <returns>A HttpResponseMessage representing the response</returns>
        public async Task<HttpResponseMessage> ExecuteRestRequestAsync(
            string operationName,
            string apiUrl,
            HttpMethod httpMethod,
            Func<HttpContent>? contentFactory = null)
        {
            _logger.LogInformation("{OperationName} started.", operationName);

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                _logger.LogWarning("{OperationName} failed: API URL is null or empty.", operationName);

                throw new RequestValidationFailureException(
                    ErrorCodes.HTTP_MISSING_API_URL,
                    new List<string>
                    {
                        $"{operationName} operation failed because the API URL is empty or misconfigured."
                    },
                    "CustomRestClient.cs / Executing ExecuteRestRequestAsync"
                );
            }

            HttpResponseMessage? response = await _customHTTPClient.SendAsync(httpMethod, apiUrl, contentFactory);

            _logger.LogInformation("{OperationName} completed.", operationName);

            return response;
        }

        /// <summary>
        /// Executes a REST API request and returns a wrapped response in CustomHttpResponseMessage.
        /// This is the primary method for atomic handlers to use for all HTTP methods.
        /// </summary>
        /// <param name="operationName">Name of the REST operation (used for logging)</param>
        /// <param name="apiUrl">The endpoint URL of the REST API</param>
        /// <param name="httpMethod">The HTTP method (GET, POST, PUT, DELETE, etc.)</param>
        /// <param name="contentFactory">Factory function to create the HTTP content (optional for GET requests)</param>
        /// <param name="username">Username for basic authentication (optional)</param>
        /// <param name="password">Password for basic authentication (optional)</param>
        /// <param name="queryParameters">Query parameters to append to URL (optional)</param>
        /// <param name="customHeaders">Additional custom headers (optional)</param>
        /// <returns>A CustomHttpResponseMessage containing the REST API response</returns>
        public async Task<HttpResponseSnapshot> ExecuteCustomRestRequestAsync(
            string operationName,
            string apiUrl,
            HttpMethod httpMethod,
            Func<HttpContent>? contentFactory = null,
            string? bearerToken = null,
            string? username = null,
            string? password = null,
            Dictionary<string, string>? queryParameters = null,
            Dictionary<string, string>? customHeaders = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            _logger.Info($"Final {operationName} started for URL: {apiUrl}");

            // Build URL with query parameters if provided
            _logger.Debug("Building URL with query parameters");
            string finalUrl = BuildUrlWithQueryParameters(apiUrl, queryParameters);

            // Prepare request headers (separate from content headers)
            List<Tuple<string, string>> requestHeaders = new List<Tuple<string, string>>();

            // Add authentication header if credentials provided
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                _logger.Debug("Adding Basic Authentication header");
                string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                requestHeaders.Add(new Tuple<string, string>("Authorization", $"Basic {authValue}"));
            }

            // Add authentication header if bearer token provided
            if (!string.IsNullOrEmpty(bearerToken))
            {
                _logger.Debug("Adding Beaer token Authentication header");
                requestHeaders.Add(new Tuple<string, string>("Authorization", $"Bearer {bearerToken}"));
            }

            // Add other request headers from customHeaders
            if (customHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in customHeaders)
                {
                    requestHeaders.Add(new Tuple<string, string>(header.Key, header.Value));
                }
            }

            // Create content factory for content headers only
            Func<HttpContent>? finalContentFactory = null;
            Dictionary<string, string>? contentHeaders = customHeaders?.Where(h => IsValidContentHeader(h.Key)).ToDictionary(h => h.Key, h => h.Value);

            if (contentFactory != null)
            {
                finalContentFactory = () =>
                {
                    HttpContent content = contentFactory();
                    return AddCustomHeaders(content, contentHeaders);
                };
            }
            else if (contentHeaders?.Count > 0)
            {
                finalContentFactory = () => CreateContentWithCustomHeaders(contentHeaders);
            }

            // Use the new SendAsync overload that accepts request headers
            _logger.Info($"Sending {httpMethod.Method} request to finalUrl: {finalUrl}");
            HttpResponseMessage response = await _customHTTPClient.SendAsync(httpMethod, finalUrl, contentFactory, requestHeaders.Count > 0 ? requestHeaders : null);

            _logger.Debug("Converting response to HttpResponseSnapshot");
            HttpResponseSnapshot result = await HttpResponseSnapshot.FromAsync(response);

            sw.Stop();

            ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{sw.ElapsedMilliseconds},");

            _logger.Info($"final {operationName} completed.");

            return result;
        }

        /// <summary>
        /// Executes a GET request to retrieve data from a REST API
        /// </summary>
        /// <param name="operationName">Name of the operation (used for logging)</param>
        /// <param name="apiUrl">The endpoint URL</param>
        /// <param name="queryParameters">Optional query parameters to append to the URL</param>
        /// <param name="username">Username for basic authentication (optional)</param>
        /// <param name="password">Password for basic authentication (optional)</param>
        /// <param name="customHeaders">Additional custom headers (optional)</param>
        /// <returns>A CustomHttpResponseMessage containing the response</returns>
        public async Task<HttpResponseSnapshot> GetAsync(
            string operationName,
            string apiUrl,
            Dictionary<string, string>? queryParameters = null,
            string? username = null,
            string? password = null,
            Dictionary<string, string>? customHeaders = null)
        {
            var finalUrl = BuildUrlWithQueryParameters(apiUrl, queryParameters);

            return await ExecuteCustomRestRequestAsync(
                operationName,
                finalUrl,
                HttpMethod.Get,
                () => CreateAuthenticatedContent(username, password, customHeaders)
            );
        }

        /// <summary>
        /// Executes a GET request with basic authentication and custom headers
        /// </summary>
        /// <param name="operationName">Name of the operation (used for logging)</param>
        /// <param name="apiUrl">The endpoint URL</param>
        /// <param name="username">Username for basic authentication</param>
        /// <param name="password">Password for basic authentication</param>
        /// <param name="queryParameters">Optional query parameters to append to the URL</param>
        /// <param name="customHeaders">Additional custom headers (optional)</param>
        /// <returns>A CustomHttpResponseMessage containing the response</returns>
        public async Task<HttpResponseSnapshot> GetWithAuthAsync(
            string operationName,
            string apiUrl,
            string username,
            string password,
            Dictionary<string, string>? queryParameters = null,
            Dictionary<string, string>? customHeaders = null)
        {
            var finalUrl = BuildUrlWithQueryParameters(apiUrl, queryParameters);

            return await ExecuteCustomRestRequestAsync(
                operationName,
                finalUrl,
                HttpMethod.Get,
                () => CreateAuthenticatedContent(username, password, customHeaders)
            );
        }

        /// <summary>
        /// Executes a POST request with JSON data
        /// </summary>
        /// <param name="operationName">Name of the operation (used for logging)</param>
        /// <param name="apiUrl">The endpoint URL</param>
        /// <param name="data">The data object to serialize and send as JSON</param>
        /// <param name="headers">Optional custom headers</param>
        /// <returns>A CustomHttpResponseMessage containing the response</returns>
        public async Task<HttpResponseSnapshot> PostJsonAsync<T>(
            string operationName,
            string apiUrl,
            T data,
            Dictionary<string, string>? headers = null)
        {
            return await ExecuteCustomRestRequestAsync(
                operationName,
                apiUrl,
                HttpMethod.Post,
                () => CreateContentWithHeaders(CreateJsonContent(data), headers)
            );
        }

        /// <summary>
        /// Executes a PUT request with JSON data
        /// </summary>
        /// <param name="operationName">Name of the operation (used for logging)</param>
        /// <param name="apiUrl">The endpoint URL</param>
        /// <param name="data">The data object to serialize and send as JSON</param>
        /// <param name="headers">Optional custom headers</param>
        /// <returns>A CustomHttpResponseMessage containing the response</returns>
        public async Task<HttpResponseSnapshot> PutJsonAsync<T>(
            string operationName,
            string apiUrl,
            T data,
            Dictionary<string, string>? headers = null)
        {
            return await ExecuteCustomRestRequestAsync(
                operationName,
                apiUrl,
                HttpMethod.Put,
                () => CreateContentWithHeaders(CreateJsonContent(data), headers)
            );
        }

        /// <summary>
        /// Executes a DELETE request
        /// </summary>
        /// <param name="operationName">Name of the operation (used for logging)</param>
        /// <param name="apiUrl">The endpoint URL</param>
        /// <returns>A CustomHttpResponseMessage containing the response</returns>
        public async Task<HttpResponseSnapshot> DeleteAsync(
            string operationName,
            string apiUrl)
        {
            return await ExecuteCustomRestRequestAsync(
                operationName,
                apiUrl,
                HttpMethod.Delete
            );
        }

        /// <summary>
        /// Creates HTTP content with basic authentication and custom headers
        /// </summary>
        /// <param name="username">Username for basic authentication (optional)</param>
        /// <param name="password">Password for basic authentication (optional)</param>
        /// <param name="customHeaders">Additional custom headers (optional)</param>
        /// <returns>HttpContent with authentication and headers applied</returns>
        private static HttpContent CreateAuthenticatedContent(string? username = null, string? password = null, Dictionary<string, string>? customHeaders = null)
        {
            // For GET requests, we typically don't send body content
            var content = new StringContent("", Encoding.UTF8, "application/json");

            return AddAuthenticationAndHeaders(content, username, password, customHeaders);
        }

        /// <summary>
        /// Adds custom headers to existing HttpContent (excluding request headers)
        /// </summary>
        /// <param name="content">The existing HttpContent</param>
        /// <param name="customHeaders">Dictionary of custom headers to add</param>
        /// <returns>HttpContent with custom headers applied</returns>
        private static HttpContent AddCustomHeaders(HttpContent content, Dictionary<string, string>? customHeaders)
        {
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    // Only add headers that are valid for content
                    if (IsValidContentHeader(header.Key))
                    {
                        try
                        {
                            content.Headers.Add(header.Key, header.Value);
                        }
                        catch (InvalidOperationException)
                        {
                            // Skip headers that cannot be added to content
                            // These should be handled at the request level
                        }
                    }
                }
            }

            return content;
        }

        /// <summary>
        /// Creates HTTP content with custom headers for requests without body content
        /// </summary>
        /// <param name="customHeaders">Dictionary of custom headers to add</param>
        /// <returns>HttpContent with custom headers applied</returns>
        private static HttpContent CreateContentWithCustomHeaders(Dictionary<string, string>? customHeaders)
        {
            // For GET requests, we typically don't send body content
            var content = new StringContent("", Encoding.UTF8, "application/json");

            return AddCustomHeaders(content, customHeaders);
        }

        /// <summary>
        /// Adds custom headers to existing HttpContent (legacy method for backward compatibility)
        /// </summary>
        /// <param name="content">The existing HttpContent</param>
        /// <param name="username">Username for basic authentication (not used - kept for compatibility)</param>
        /// <param name="password">Password for basic authentication (not used - kept for compatibility)</param>
        /// <param name="customHeaders">Additional custom headers (optional)</param>
        /// <returns>HttpContent with custom headers applied</returns>
        private static HttpContent AddAuthenticationAndHeaders(HttpContent content, string? username = null, string? password = null, Dictionary<string, string>? customHeaders = null)
        {
            // Note: Authorization header cannot be added to content headers as it's a request header
            // This method now delegates to AddCustomHeaders for consistency
            // Authentication is handled separately in ExecuteCustomRestRequestAsync

            return AddCustomHeaders(content, customHeaders);
        }

        /// <summary>
        /// Determines if a header is valid for content headers
        /// </summary>
        /// <param name="headerName">The header name to check</param>
        /// <returns>True if it's a valid content header, false if it's a request header</returns>
        private static bool IsValidContentHeader(string headerName)
        {
            // Common request headers that should NOT be added to content headers
            var requestHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Authorization",
                "Accept",
                "Accept-Charset",
                "Accept-Encoding",
                "Accept-Language",
                "Cache-Control",
                "Effective-Of",
                "Connection",
                "Cookie",
                "Host",
                "If-Match",
                "If-Modified-Since",
                "If-None-Match",
                "If-Range",
                "If-Unmodified-Since",
                "Max-Forwards",
                "Pragma",
                "Proxy-Authorization",
                "Range",
                "Referer",
                "TE",
                "Upgrade",
                "User-Agent",
                "Via",
                "Warning"
            };

            return !requestHeaders.Contains(headerName);
        }

        /// <summary>
        /// Builds a URL with query parameters
        /// </summary>
        /// <param name="baseUrl">The base URL</param>
        /// <param name="queryParameters">Dictionary of query parameters</param>
        /// <returns>URL with query parameters appended</returns>
        private static string BuildUrlWithQueryParameters(string baseUrl, Dictionary<string, string>? queryParameters)
        {
            if (queryParameters == null || queryParameters.Count == 0)
                return baseUrl;

            string queryString = string.Join("&",
                  queryParameters.Select(kvp =>
                  {
                      string key = Uri.EscapeDataString(kvp.Key);
                      string value = Uri.EscapeDataString(kvp.Value)
                        .Replace("%3B", ";")
                        .Replace("%3D", "=")
                        .Replace("%2C", ",")
                        .Replace("%7B", "{")
                        .Replace("%7D", "}");
                      return $"{key}={value}";
                  }));

            string separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}{queryString}";
        }
    }
}
