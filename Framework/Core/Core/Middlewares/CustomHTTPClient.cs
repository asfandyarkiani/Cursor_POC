using Core.Extensions;
using Core.Headers;
using Microsoft.Extensions.Logging;
using Polly;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace Core.Middlewares
{
    public class CustomHTTPClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomHTTPClient> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _httpPolicy;
        private static readonly string[] ResHeadersToBeAdded = { ResponseHeaders.SYSTotalTime.Item1, ResponseHeaders.DSTimeBreakDown.Item1, ResponseHeaders.IsDownStreamError.Item1 };

        public CustomHTTPClient(HttpClient httpClient, ILogger<CustomHTTPClient> logger, IAsyncPolicy<HttpResponseMessage> httpPolicy)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(50);
            _logger = logger;
            _httpPolicy = httpPolicy;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, Func<HttpContent?> contentFactory)
        {
            if (method is null) throw new ArgumentNullException(nameof(method));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("url is required", nameof(url));
            try
            {
                HttpContent? content = contentFactory?.Invoke();

                if (content is not null)
                {
                    string requestBody = await content.ReadAsStringAsync();
                    _logger.Critical($"[HTTP REQUEST] {method} {url}\nBody:\n{requestBody}");
                }
                else
                {
                    _logger.Critical($"[HTTP REQUEST] {method} {url}\nBody: <no body>");
                }

                HttpResponseMessage response = await _httpPolicy.ExecuteAsync(async () =>
                {
                    using HttpRequestMessage request = new HttpRequestMessage(method, url)
                    {
                        Content = contentFactory?.Invoke()
                    };
                    return await _httpClient.SendAsync(request);
                });

                string responseBody = await ReadAsStringOrGzipBytesAsync(response.Content);
                _logger.Critical($"[HTTP RESPONSE] {method} {url}\nStatus: {(int)response.StatusCode} {response.StatusCode}\nResponse:\n{responseBody}");

                _logger.Info($"Request to {url} completed with status code {response.StatusCode}.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Exception occurred during request to {url}: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, Func<HttpContent?> contentFactory, List<Tuple<string,string>> reqHeaders = null, List<Tuple<string, string>> resHeaders = null)
        {
            if (method is null) throw new ArgumentNullException(nameof(method));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("url is required", nameof(url));

            try
            {
                HttpContent? content = contentFactory?.Invoke();

                if (content is not null)
                {
                    string requestBody = await content.ReadAsStringAsync();
                    _logger.Critical($"[HTTP REQUEST] {method} {url}\nBody:\n{requestBody}");
                }
                else
                {
                    _logger.Critical($"[HTTP REQUEST] {method} {url}\nBody: <no body>");
                }
                HttpResponseMessage response = await _httpPolicy.ExecuteAsync(async () =>
                {
                    using HttpRequestMessage request = new HttpRequestMessage(method, url)
                    {
                        Content = contentFactory?.Invoke()
                    };

                    if (reqHeaders?.Count > 0)
                    {
                        foreach (Tuple<String,String> header in reqHeaders)
                        {
                            String? headerName = header?.Item1;
                            String? headerValue = header?.Item2;
                            if (string.IsNullOrEmpty(headerName) || string.IsNullOrEmpty(headerValue)) continue;

                            if (header.IsContentHeader())
                            {
                                if (request.Content == null) continue;
                                request.Content.Headers.TryAddWithoutValidation(headerName, headerValue);
                            }
                            else
                            {
                                request.Headers.TryAddWithoutValidation(headerName, headerValue);
                            }
                        }
                    }
                    return await _httpClient.SendAsync(request);
                });

                if (resHeaders?.Count > 0)
                {
                    foreach (var header in resHeaders)
                    {
                        String? headerName = header.Item1;
                        String? headerValue = header.Item2;
                        if (string.IsNullOrWhiteSpace(headerName) || headerValue is null) continue;

                        if (header.IsContentHeader())
                        {
                            if (response.Content == null) continue;
                            response.Content.Headers.TryAddWithoutValidation(headerName, headerValue);
                        }
                        else
                        {
                            response.Headers.TryAddWithoutValidation(headerName, headerValue);
                        }
                    }
                }

                string responseBody = await ReadAsStringOrGzipBytesAsync(response.Content);
                _logger.Critical($"[HTTP RESPONSE] {method} {url}\nStatus: {(int)response.StatusCode} {response.StatusCode}\nResponse:\n{responseBody}");

                _logger.Info($"Request to {url} completed with status code {response.StatusCode}.");

                CaptureHeaders(response);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Exception occurred during request to {url}: {ex.Message}");
                throw;
            }
        }

        private void CaptureHeaders(HttpResponseMessage response)
        {
            if (response is null) return;
            
            HttpResponseHeaders headers = response.Headers;
            foreach (string header in ResHeadersToBeAdded)
            {
                if (!headers.TryGetValues(header, out IEnumerable<string>? values))
                    continue;

                string? headerValue = values.FirstOrDefault();
                if (string.IsNullOrEmpty(headerValue))
                    continue;

                if (header == ResponseHeaders.SYSTotalTime.Item1)
                {
                    if (int.TryParse(headerValue, out int current))
                    {
                        StringBuilder sysTotalTime = ResponseHeaders.SYSTotalTime.Item2.Value ??= new StringBuilder();
                        int.TryParse(sysTotalTime.ToString(), out int previous);
                        sysTotalTime.Clear().Append(previous + current);
                    }
                }
                else if (header == ResponseHeaders.DSTimeBreakDown.Item1)
                {
                    ResponseHeaders.DSTimeBreakDown.Item2.Value ??= new StringBuilder();
                    if (ResponseHeaders.DSTimeBreakDown.Item2.Value.Length > 0)
                        ResponseHeaders.DSTimeBreakDown.Item2.Value.Append(',');
                    ResponseHeaders.DSTimeBreakDown.Item2.Value.Append(headerValue);
                }
                else if (header == ResponseHeaders.IsDownStreamError.Item1)
                {
                    ResponseHeaders.IsDownStreamError.Item2.Value ??= new StringBuilder();

                    bool.TryParse(headerValue, out bool isDownStreamError);

                    ResponseHeaders.IsDownStreamError.Item2.Value.Clear();
                    ResponseHeaders.IsDownStreamError.Item2.Value.Append(isDownStreamError.ToString());
                }
            }
        }

        private async Task<string> ReadAsStringOrGzipBytesAsync(HttpContent? content)
        {
            if (content == null)
                return string.Empty;

            // Step 1: Try read as string
            string text = await content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(text))
                return text;

            // Step 2: Read raw bytes
            byte[] rawBytes = await content.ReadAsByteArrayAsync();

            if (rawBytes == null || rawBytes.Length == 0)
                return string.Empty;

            // Step 3: Try decompress (gzip). If invalid gzip, fallback to UTF-8 decode.
            if (IsGzipCompressed(rawBytes))
            {
                string decompressed = UnzipStepError(rawBytes);
                if (!string.IsNullOrWhiteSpace(decompressed))
                    return decompressed;
            }

            // Step 4: Fallback plain text decode
            return Encoding.UTF8.GetString(rawBytes);
        }

        private bool IsGzipCompressed(byte[] data)
        {
            return data.Length > 2 && data[0] == 0x1F && data[1] == 0x8B;
        }

        private string UnzipStepError(byte[] compressedBytes)
        {
            using var inputStream = new MemoryStream(compressedBytes);
            using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();

            gzipStream.CopyTo(outputStream);
            byte[] decompressedBytes = outputStream.ToArray();

            return Encoding.UTF8.GetString(decompressedBytes);
        }
    }
}
