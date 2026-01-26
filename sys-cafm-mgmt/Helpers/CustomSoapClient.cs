using Core.Extensions;
using Core.Headers;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace CAFMSystem.Helpers
{
    /// <summary>
    /// Custom SOAP client for executing SOAP requests with performance timing.
    /// Wraps CustomHTTPClient and tracks operation timing in ResponseHeaders.DSTimeBreakDown.
    /// </summary>
    public class CustomSoapClient
    {
        private readonly CustomHTTPClient _customHTTPClient;
        private readonly ILogger<CustomSoapClient> _logger;

        public CustomSoapClient(CustomHTTPClient customHTTPClient, ILogger<CustomSoapClient> logger)
        {
            _customHTTPClient = customHTTPClient;
            _logger = logger;
        }

        /// <summary>
        /// Executes a SOAP request with timing tracking.
        /// </summary>
        /// <param name="operationName">Operation name for timing tracking</param>
        /// <param name="soapEnvelope">SOAP envelope XML</param>
        /// <param name="apiUrl">SOAP endpoint URL</param>
        /// <param name="soapActionUrl">SOAPAction header value</param>
        /// <param name="httpMethod">HTTP method (typically POST)</param>
        /// <returns>HttpResponseSnapshot containing response</returns>
        public async Task<HttpResponseSnapshot> ExecuteCustomSoapRequestAsync(
            string operationName,
            string soapEnvelope,
            string apiUrl,
            string soapActionUrl,
            HttpMethod httpMethod)
        {
            _logger.Info($"Executing SOAP operation: {operationName}");
            
            Stopwatch sw = Stopwatch.StartNew();
            
            HttpResponseMessage response = await ExecuteSoapRequestAsync(
                apiUrl,
                soapEnvelope,
                soapActionUrl,
                httpMethod);
            
            HttpResponseSnapshot result = await HttpResponseSnapshot.FromAsync(response);
            
            sw.Stop();
            ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{sw.ElapsedMilliseconds},");
            
            _logger.Info($"SOAP operation {operationName} completed in {sw.ElapsedMilliseconds}ms with status: {result.StatusCode}");
            
            return result;
        }

        /// <summary>
        /// Executes the actual SOAP HTTP request.
        /// </summary>
        private async Task<HttpResponseMessage> ExecuteSoapRequestAsync(
            string apiUrl,
            string soapEnvelope,
            string soapActionUrl,
            HttpMethod httpMethod)
        {
            StringContent content = CreateSoapContent(soapEnvelope, soapActionUrl);
            
            List<Tuple<string, string>> headers = new List<Tuple<string, string>>();
            
            HttpResponseMessage response = await _customHTTPClient.SendAsync(
                httpMethod,
                apiUrl,
                () => content,
                headers);
            
            return response;
        }

        /// <summary>
        /// Creates StringContent for SOAP request with proper headers.
        /// </summary>
        public static StringContent CreateSoapContent(string soapEnvelope, string soapActionUrl)
        {
            StringContent content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            content.Headers.Remove("Content-Type");
            content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
            content.Headers.Add("SOAPAction", soapActionUrl);
            return content;
        }
    }
}
