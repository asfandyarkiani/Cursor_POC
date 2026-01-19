using System.Text;
using Microsoft.Extensions.Logging;
using Polly;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Custom SOAP client for making SOAP over HTTP requests.
/// </summary>
public class CustomSoapClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomSoapClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public CustomSoapClient(
        HttpClient httpClient,
        ILogger<CustomSoapClient> logger,
        IAsyncPolicy<HttpResponseMessage> retryPolicy)
    {
        _httpClient = httpClient;
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    /// <summary>
    /// Sends a SOAP request and returns the response.
    /// </summary>
    /// <param name="url">The endpoint URL.</param>
    /// <param name="soapEnvelope">The SOAP envelope XML.</param>
    /// <param name="soapAction">The SOAP action header value.</param>
    /// <returns>An HttpResponseSnapshot containing the response details.</returns>
    public async Task<HttpResponseSnapshot> SendSoapRequestAsync(
        string url, 
        string soapEnvelope, 
        string soapAction)
    {
        _logger.Info("Sending SOAP request to {Url} with action {SoapAction}", url, soapAction);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            request.Headers.Add("SOAPAction", soapAction);

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
                _logger.Error("SOAP request failed. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    snapshot.StatusCode, snapshot.ReasonPhrase);
            }
            else
            {
                // Check for SOAP fault even in 200 response
                if (SOAPHelper.ContainsSoapFault(content))
                {
                    var faultMessage = SOAPHelper.ExtractSoapFaultMessage(content);
                    _logger.Error("SOAP fault received: {FaultMessage}", faultMessage);
                    snapshot.IsSoapFault = true;
                    snapshot.SoapFaultMessage = faultMessage;
                }
                else
                {
                    _logger.Info("SOAP request completed successfully.");
                }
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
            _logger.Error(ex, "SOAP request timed out.");
            throw new DownStreamApiFailureException("SOAP request timed out.", ex);
        }
    }
}

/// <summary>
/// Snapshot of an HTTP response for passing between layers.
/// </summary>
public class HttpResponseSnapshot
{
    public int StatusCode { get; set; }
    public string? Content { get; set; }
    public bool IsSuccessStatusCode { get; set; }
    public string? ReasonPhrase { get; set; }
    public bool IsSoapFault { get; set; }
    public string? SoapFaultMessage { get; set; }
}
