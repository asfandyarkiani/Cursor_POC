using Core.Extensions;
using Core.Headers;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using CAFMSystemLayer.ConfigModels;

namespace CAFMSystemLayer.Helper
{
    /// <summary>
    /// Custom SOAP client for executing SOAP requests with performance timing tracking.
    /// MANDATORY: Implements timing tracking (Stopwatch + ResponseHeaders.DSTimeBreakDown).
    /// </summary>
    public class CustomSoapClient
    {
        private readonly CustomHTTPClient _customHTTPClient;
        private readonly AppConfigs _appConfigs;
        private readonly ILogger<CustomSoapClient> _logger;
        
        public CustomSoapClient(CustomHTTPClient customHTTPClient, IOptions<AppConfigs> options, ILogger<CustomSoapClient> logger)
        {
            _customHTTPClient = customHTTPClient;
            _appConfigs = options.Value;
            _logger = logger;
        }
        
        /// <summary>
        /// Executes SOAP request with performance timing tracking.
        /// </summary>
        /// <param name="operationName">Operation name for timing tracking (use OperationNames constants)</param>
        /// <param name="soapEnvelope">Complete SOAP envelope XML</param>
        /// <param name="apiUrl">Full API URL (base + endpoint)</param>
        /// <param name="soapActionUrl">SOAPAction header value</param>
        /// <param name="httpMethod">HTTP method (typically POST)</param>
        /// <returns>HttpResponseSnapshot with timing tracked</returns>
        public async Task<HttpResponseSnapshot> ExecuteCustomSoapRequestAsync(
            string operationName, 
            string soapEnvelope, 
            string? apiUrl, 
            string? soapActionUrl, 
            HttpMethod httpMethod)
        {
            _logger.Info($"Executing SOAP request: {operationName}");
            
            // MANDATORY: Start stopwatch before call
            Stopwatch sw = Stopwatch.StartNew();
            
            try
            {
                HttpResponseMessage response = await ExecuteSoapRequestAsync(
                    soapEnvelope, 
                    apiUrl, 
                    soapActionUrl, 
                    httpMethod
                );
                
                HttpResponseSnapshot result = await HttpResponseSnapshot.FromAsync(response);
                
                // MANDATORY: Stop stopwatch after response
                sw.Stop();
                
                // MANDATORY: Append timing to ResponseHeaders.DSTimeBreakDown
                ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{sw.ElapsedMilliseconds},");
                
                _logger.Info($"SOAP request completed: {operationName} - Status: {result.StatusCode} - Time: {sw.ElapsedMilliseconds}ms");
                
                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.Error(ex, $"SOAP request failed: {operationName} - Time: {sw.ElapsedMilliseconds}ms");
                throw;
            }
        }
        
        private async Task<HttpResponseMessage> ExecuteSoapRequestAsync(
            string soapEnvelope, 
            string? apiUrl, 
            string? soapActionUrl, 
            HttpMethod httpMethod)
        {
            if (string.IsNullOrWhiteSpace(apiUrl))
                throw new ArgumentException("API URL cannot be null or empty", nameof(apiUrl));
                
            StringContent content = CreateSoapContent(soapEnvelope, soapActionUrl);
            
            List<Tuple<string, string>> headers = new List<Tuple<string, string>>
            {
                RequestHeaders.TestRunId.Simplify(),
                RequestHeaders.RequestId.Simplify()
            };
            
            return await _customHTTPClient.SendAsync(httpMethod, apiUrl, () => content, headers);
        }
        
        /// <summary>
        /// Creates StringContent for SOAP request with proper headers.
        /// </summary>
        public static StringContent CreateSoapContent(string soapEnvelope, string? soapActionUrl)
        {
            StringContent content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            
            if (!string.IsNullOrWhiteSpace(soapActionUrl))
            {
                content.Headers.Add("SOAPAction", soapActionUrl);
            }
            
            return content;
        }
    }
}
