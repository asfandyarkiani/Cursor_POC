using FacilitiesMgmtSystem.Core.System.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Custom HTTP client wrapper with Polly resilience policies.
/// </summary>
public class CustomHTTPClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomHTTPClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage>? _retryPolicy;

    public CustomHTTPClient(
        HttpClient httpClient,
        ILogger<CustomHTTPClient> logger,
        IAsyncPolicy<HttpResponseMessage>? retryPolicy = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    public async Task<HttpResponseSnapshot> GetAsync(string url, Dictionary<string, string>? headers = null)
    {
        return await SendAsync(HttpMethod.Get, url, null, headers);
    }

    public async Task<HttpResponseSnapshot> PostAsync(string url, string content, Dictionary<string, string>? headers = null)
    {
        return await SendAsync(HttpMethod.Post, url, content, headers);
    }

    public async Task<HttpResponseSnapshot> SendAsync(
        HttpMethod method,
        string url,
        string? content = null,
        Dictionary<string, string>? headers = null)
    {
        try
        {
            var request = new HttpRequestMessage(method, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response;
            if (_retryPolicy != null)
            {
                response = await _retryPolicy.ExecuteAsync(() => _httpClient.SendAsync(request));
            }
            else
            {
                response = await _httpClient.SendAsync(request);
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            var snapshot = new HttpResponseSnapshot
            {
                StatusCode = response.StatusCode,
                ResponseBody = responseBody
            };

            foreach (var header in response.Headers)
            {
                snapshot.Headers[header.Key] = string.Join(",", header.Value);
            }

            return snapshot;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed: {Url}", url);
            throw new DownStreamApiFailureException($"HTTP request to {url} failed: {ex.Message}", ex);
        }
    }
}
