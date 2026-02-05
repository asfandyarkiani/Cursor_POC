using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemLayer.Application.Interfaces;
using SystemLayer.Infrastructure.Configuration;
using SystemLayer.Infrastructure.Models;
using SystemLayer.Infrastructure.Soap;

namespace SystemLayer.Infrastructure.Services;

/// <summary>
/// CAFM client implementation for SOAP/XML operations
/// </summary>
public class CafmClient : ICafmClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CafmClient> _logger;
    private readonly CafmOptions _options;
    private readonly SoapMessageBuilder _soapBuilder;
    
    public CafmClient(
        HttpClient httpClient,
        ILogger<CafmClient> logger,
        IOptions<CafmOptions> options,
        SoapMessageBuilder soapBuilder)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        _soapBuilder = soapBuilder;
        
        ConfigureHttpClient();
    }
    
    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        _httpClient.DefaultRequestHeaders.Add("SOAPAction", "");
    }
    
    /// <inheritdoc />
    public async Task<string> LoginAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initiating CAFM login with correlation {CorrelationId}", correlationId);
        
        try
        {
            var loginRequest = new CafmLoginRequest
            {
                Username = _options.Username,
                Password = _options.Password,
                Database = _options.Database
            };
            
            var requestXml = _soapBuilder.SerializeToXml(loginRequest, correlationId);
            var soapEnvelope = _soapBuilder.BuildSoapEnvelope("Login", requestXml, correlationId);
            
            var responseXml = await SendSoapRequestInternalAsync("Login", soapEnvelope, correlationId, cancellationToken);
            
            if (_soapBuilder.ContainsSoapFault(responseXml, correlationId))
            {
                var (faultCode, faultString, detail) = _soapBuilder.ExtractSoapFault(responseXml, correlationId);
                throw new InvalidOperationException($"CAFM login failed - {faultCode}: {faultString}. Detail: {detail}");
            }
            
            var bodyXml = _soapBuilder.ExtractSoapBody(responseXml, correlationId);
            var loginResponse = _soapBuilder.DeserializeFromXml<CafmLoginResponse>(bodyXml, correlationId);
            
            if (!loginResponse.Success || string.IsNullOrEmpty(loginResponse.SessionToken))
            {
                throw new InvalidOperationException($"CAFM login failed: {loginResponse.Message}");
            }
            
            _logger.LogInformation("CAFM login successful with correlation {CorrelationId}", correlationId);
            return loginResponse.SessionToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CAFM login failed with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task LogoutAsync(string sessionToken, string correlationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initiating CAFM logout with correlation {CorrelationId}", correlationId);
        
        try
        {
            var logoutRequest = new CafmLogoutRequest
            {
                SessionToken = sessionToken
            };
            
            var requestXml = _soapBuilder.SerializeToXml(logoutRequest, correlationId);
            var soapEnvelope = _soapBuilder.BuildSoapEnvelope("Logout", requestXml, correlationId);
            
            var responseXml = await SendSoapRequestInternalAsync("Logout", soapEnvelope, correlationId, cancellationToken);
            
            if (_soapBuilder.ContainsSoapFault(responseXml, correlationId))
            {
                var (faultCode, faultString, detail) = _soapBuilder.ExtractSoapFault(responseXml, correlationId);
                _logger.LogWarning("CAFM logout fault - {FaultCode}: {FaultString}. Detail: {Detail}. Correlation: {CorrelationId}", 
                    faultCode, faultString, detail, correlationId);
                // Don't throw on logout faults - session might already be expired
                return;
            }
            
            var bodyXml = _soapBuilder.ExtractSoapBody(responseXml, correlationId);
            var logoutResponse = _soapBuilder.DeserializeFromXml<CafmLogoutResponse>(bodyXml, correlationId);
            
            if (!logoutResponse.Success)
            {
                _logger.LogWarning("CAFM logout unsuccessful: {Message}. Correlation: {CorrelationId}", 
                    logoutResponse.Message, correlationId);
                // Don't throw - logout might fail if session already expired
            }
            else
            {
                _logger.LogInformation("CAFM logout successful with correlation {CorrelationId}", correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CAFM logout failed with correlation {CorrelationId} - continuing anyway", correlationId);
            // Don't throw on logout failures - we want to continue processing
        }
    }
    
    /// <inheritdoc />
    public async Task<TResponse> ExecuteWithSessionAsync<TRequest, TResponse>(
        string operation,
        TRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        _logger.LogInformation("Executing CAFM operation {Operation} with session wrapper. Correlation: {CorrelationId}", 
            operation, correlationId);
        
        string? sessionToken = null;
        
        try
        {
            // Step 1: Login
            sessionToken = await LoginAsync(correlationId, cancellationToken);
            
            // Step 2: Set session token in request and execute operation
            SetSessionTokenInRequest(request, sessionToken);
            var requestXml = _soapBuilder.SerializeToXml(request, correlationId);
            var soapEnvelope = _soapBuilder.BuildSoapEnvelope(operation, requestXml, correlationId);
            
            var responseXml = await SendSoapRequestAsync(operation, soapEnvelope, sessionToken, correlationId, cancellationToken);
            
            if (_soapBuilder.ContainsSoapFault(responseXml, correlationId))
            {
                var (faultCode, faultString, detail) = _soapBuilder.ExtractSoapFault(responseXml, correlationId);
                throw new InvalidOperationException($"CAFM operation {operation} failed - {faultCode}: {faultString}. Detail: {detail}");
            }
            
            var bodyXml = _soapBuilder.ExtractSoapBody(responseXml, correlationId);
            var response = _soapBuilder.DeserializeFromXml<TResponse>(bodyXml, correlationId);
            
            _logger.LogInformation("CAFM operation {Operation} completed successfully. Correlation: {CorrelationId}", 
                operation, correlationId);
            
            return response;
        }
        finally
        {
            // Step 3: Always logout (even if operation failed)
            if (!string.IsNullOrEmpty(sessionToken))
            {
                await LogoutAsync(sessionToken, correlationId, cancellationToken);
            }
        }
    }
    
    /// <inheritdoc />
    public async Task<string> SendSoapRequestAsync(
        string soapAction,
        string soapBody,
        string sessionToken,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        // For operations that require session token, we assume it's already included in the soapBody
        return await SendSoapRequestInternalAsync(soapAction, soapBody, correlationId, cancellationToken);
    }
    
    private async Task<string> SendSoapRequestInternalAsync(
        string soapAction,
        string soapEnvelope,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sending SOAP request for action {SoapAction} with correlation {CorrelationId}", 
            soapAction, correlationId);
        
        try
        {
            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            
            // Set SOAPAction header
            content.Headers.Remove("SOAPAction");
            content.Headers.Add("SOAPAction", $"\"{_options.SoapNamespace}/{soapAction}\"");
            
            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug("SOAP Request - Action: {SoapAction}, Correlation: {CorrelationId}, Body: {SoapEnvelope}", 
                    soapAction, correlationId, soapEnvelope);
            }
            
            var response = await _httpClient.PostAsync(_options.ServicePath, content, cancellationToken);
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug("SOAP Response - Action: {SoapAction}, Correlation: {CorrelationId}, Status: {StatusCode}, Body: {ResponseContent}", 
                    soapAction, correlationId, response.StatusCode, responseContent);
            }
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("SOAP request failed - Action: {SoapAction}, Correlation: {CorrelationId}, Status: {StatusCode}, Response: {ResponseContent}", 
                    soapAction, correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"SOAP request failed with status {response.StatusCode}: {responseContent}");
            }
            
            _logger.LogDebug("SOAP request completed successfully for action {SoapAction} with correlation {CorrelationId}", 
                soapAction, correlationId);
            
            return responseContent;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "SOAP request timeout for action {SoapAction} with correlation {CorrelationId}", 
                soapAction, correlationId);
            throw new TimeoutException($"SOAP request for {soapAction} timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SOAP request failed for action {SoapAction} with correlation {CorrelationId}", 
                soapAction, correlationId);
            throw;
        }
    }
    
    private static void SetSessionTokenInRequest<TRequest>(TRequest request, string sessionToken) where TRequest : class
    {
        // Use reflection to set SessionToken property if it exists
        var sessionTokenProperty = typeof(TRequest).GetProperty("SessionToken");
        if (sessionTokenProperty != null && sessionTokenProperty.CanWrite)
        {
            sessionTokenProperty.SetValue(request, sessionToken);
        }
    }
}