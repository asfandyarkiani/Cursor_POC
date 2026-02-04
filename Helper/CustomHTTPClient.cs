using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Custom HTTP client wrapper for making REST API calls with retry policies.
/// </summary>
public class CustomHTTPClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomHTTPClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public CustomHTTPClient(
        HttpClient httpClient,
        ILogger<CustomHTTPClient> logger,
        IAsyncPolicy<HttpResponseMessage> retryPolicy)
    {
        _httpClient = httpClient;
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    /// <summary>
    /// Sends an HTTP GET request.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <returns>An HttpResponseSnapshot containing the response details.</returns>
    public async Task<HttpResponseSnapshot> GetAsync(string url, Dictionary<string, string>? headers = null)
    {
        _logger.Info("Sending HTTP GET request to {Url}", url);

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddHeaders(request, headers);

        return await SendRequestAsync(request);
    }

    /// <summary>
    /// Sends an HTTP POST request with JSON content.
    /// </summary>
    /// <typeparam name="T">The type of the request body.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="body">The request body to serialize as JSON.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <returns>An HttpResponseSnapshot containing the response details.</returns>
    public async Task<HttpResponseSnapshot> PostJsonAsync<T>(string url, T body, Dictionary<string, string>? headers = null)
    {
        _logger.Info("Sending HTTP POST (JSON) request to {Url}", url);

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var json = JsonSerializer.Serialize(body);
        request.Content = new StringContent(json, Encoding.UTF8, InfoConstants.CONTENT_TYPE_JSON);
        AddHeaders(request, headers);

        return await SendRequestAsync(request);
    }

    /// <summary>
    /// Sends an HTTP POST request with string content.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="content">The string content.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <returns>An HttpResponseSnapshot containing the response details.</returns>
    public async Task<HttpResponseSnapshot> PostAsync(string url, string content, string contentType, Dictionary<string, string>? headers = null)
    {
        _logger.Info("Sending HTTP POST request to {Url}", url);

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(content, Encoding.UTF8, contentType);
        AddHeaders(request, headers);

        return await SendRequestAsync(request);
    }

    private async Task<HttpResponseSnapshot> SendRequestAsync(HttpRequestMessage request)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.SendAsync(request);
            });

            var content = await response.Content.ReadAsStringAsync();

            var snapshot = new HttpResponseSnapshot
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase
            };

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("HTTP request failed. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    snapshot.StatusCode, snapshot.ReasonPhrase);
            }
            else
            {
                _logger.Info("HTTP request completed successfully.");
            }

            return snapshot;
        }
        catch (HttpRequestException ex)
        {
            _logger.Error(ex, "HTTP request failed: {Message}", ex.Message);
            throw new DownStreamApiFailureException(ErrorConstants.HTTP_REQUEST_FAILED, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.Error(ex, "HTTP request timed out.");
            throw new DownStreamApiFailureException("HTTP request timed out.", ex);
        }
    }

    private void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}
