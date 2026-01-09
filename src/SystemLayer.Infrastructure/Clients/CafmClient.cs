using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemLayer.Application.Configuration;
using SystemLayer.Application.Interfaces;
using System.Text;

namespace SystemLayer.Infrastructure.Clients;

public class CafmClient : ICafmClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CafmClient> _logger;
    private readonly CafmConfiguration _config;

    public CafmClient(
        HttpClient httpClient,
        ILogger<CafmClient> logger,
        IOptions<CafmConfiguration> config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<string> SendSoapRequestAsync(
        string soapAction, 
        string xmlRequest, 
        string? sessionToken = null, 
        CancellationToken cancellationToken = default)
    {
        using var activity = _logger.BeginScope("CAFM SOAP Request: {SoapAction}", soapAction);
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _config.BaseUrl)
            {
                Content = new StringContent(xmlRequest, Encoding.UTF8, _config.Soap.ContentType)
            };

            request.Headers.Add("SOAPAction", $"\"{_config.Soap.SoapNamespace}{soapAction}\"");
            
            if (!string.IsNullOrEmpty(sessionToken))
            {
                request.Headers.Add("Authorization", $"Bearer {sessionToken}");
            }

            _logger.LogDebug("Sending SOAP request to CAFM. Action: {SoapAction}", soapAction);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CAFM SOAP request failed. Action: {SoapAction}, StatusCode: {StatusCode}, Response: {Response}",
                    soapAction, response.StatusCode, responseContent);
                
                throw new HttpRequestException(
                    $"CAFM request failed with status {response.StatusCode}: {responseContent}");
            }

            _logger.LogDebug("CAFM SOAP request successful. Action: {SoapAction}", soapAction);
            return responseContent;
        }
        catch (Exception ex) when (!(ex is HttpRequestException))
        {
            _logger.LogError(ex, "Error sending SOAP request to CAFM. Action: {SoapAction}", soapAction);
            throw;
        }
    }
}