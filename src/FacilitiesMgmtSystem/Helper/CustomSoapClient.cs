using System.Reflection;
using System.Text;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.System.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Custom SOAP client for making SOAP calls with embedded envelope templates.
/// </summary>
public class CustomSoapClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomSoapClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage>? _retryPolicy;

    public CustomSoapClient(
        HttpClient httpClient,
        ILogger<CustomSoapClient> logger,
        IAsyncPolicy<HttpResponseMessage>? retryPolicy = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    /// <summary>
    /// Sends a SOAP request using the specified envelope template.
    /// </summary>
    /// <param name="url">The SOAP endpoint URL.</param>
    /// <param name="soapAction">The SOAP action header value.</param>
    /// <param name="envelopeTemplate">The SOAP envelope XML template.</param>
    /// <param name="replacements">Dictionary of placeholder replacements.</param>
    /// <returns>The HTTP response snapshot.</returns>
    public async Task<HttpResponseSnapshot> SendSoapRequestAsync(
        string url,
        string soapAction,
        string envelopeTemplate,
        Dictionary<string, string>? replacements = null)
    {
        try
        {
            var envelope = envelopeTemplate;
            if (replacements != null)
            {
                foreach (var replacement in replacements)
                {
                    envelope = envelope.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
                }
            }

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(envelope, Encoding.UTF8, "text/xml");
            request.Headers.Add("SOAPAction", soapAction);

            _logger.LogDebug("Sending SOAP request to {Url} with action {SoapAction}", url, soapAction);

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

            if (!response.IsSuccessStatusCode)
            {
                var faultMessage = SOAPHelper.ExtractSoapFault(responseBody);
                _logger.LogWarning("SOAP request failed. Status: {StatusCode}, Fault: {Fault}", 
                    response.StatusCode, faultMessage ?? "Unknown");
            }

            return snapshot;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "SOAP request to {Url} failed", url);
            throw new DownStreamApiFailureException($"SOAP request to {url} failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads a SOAP envelope template from embedded resources.
    /// </summary>
    /// <param name="resourceName">The name of the embedded resource (e.g., "SoapEnvelopes.Login.xml").</param>
    /// <returns>The envelope template string.</returns>
    public static string LoadEnvelopeTemplate(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullResourceName = $"FacilitiesMgmtSystem.{resourceName}";
        
        using var stream = assembly.GetManifestResourceStream(fullResourceName);
        if (stream == null)
        {
            throw new DownStreamApiFailureException(
                $"{ErrorConstants.SOAP_ENVELOPE_NOT_FOUND}: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
