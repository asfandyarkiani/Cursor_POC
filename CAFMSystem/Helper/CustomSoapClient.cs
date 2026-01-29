using Core.Extensions;
using Core.Headers;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CAFMSystem.Helper
{
    /// <summary>
    /// Custom SOAP client for CAFM FSI API operations.
    /// Wraps CustomHTTPClient for SOAP requests with performance timing.
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
        /// Executes SOAP request with performance timing tracking.
        /// </summary>
        /// <param name="operationName">Operation name for timing tracking</param>
        /// <param name="soapEnvelope">SOAP envelope XML</param>
        /// <param name="apiUrl">API endpoint URL</param>
        /// <param name="soapActionUrl">SOAPAction header value</param>
        /// <param name="httpMethod">HTTP method (typically POST)</param>
        /// <returns>HttpResponseSnapshot</returns>
        public async Task<HttpResponseSnapshot> ExecuteCustomSoapRequestAsync(
            string operationName,
            string soapEnvelope,
            string apiUrl,
            string soapActionUrl,
            HttpMethod httpMethod)
        {
            _logger.Info($"Executing SOAP operation: {operationName}");

            // MANDATORY: Start stopwatch before call
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                // Create SOAP content
                StringContent soapContent = CreateSoapContent(soapEnvelope, soapActionUrl);

                // Execute HTTP request
                HttpResponseMessage httpResponse = await _customHTTPClient.SendAsync(
                    httpMethod,
                    apiUrl,
                    () => soapContent,
                    headers: null
                );

                // Convert to HttpResponseSnapshot
                HttpResponseSnapshot responseSnapshot = await HttpResponseSnapshot.FromAsync(httpResponse);

                // MANDATORY: Stop stopwatch after response
                stopwatch.Stop();

                // MANDATORY: Append timing to ResponseHeaders.DSTimeBreakDown
                ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{stopwatch.ElapsedMilliseconds},");

                _logger.Info($"SOAP operation {operationName} completed in {stopwatch.ElapsedMilliseconds}ms - Status: {responseSnapshot.StatusCode}");

                return responseSnapshot;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{stopwatch.ElapsedMilliseconds},");

                _logger.Error(ex, $"SOAP operation {operationName} failed after {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }

        /// <summary>
        /// Creates StringContent for SOAP request with proper headers.
        /// </summary>
        /// <param name="soapEnvelope">SOAP envelope XML</param>
        /// <param name="soapActionUrl">SOAPAction header value</param>
        /// <returns>StringContent with SOAP envelope</returns>
        public static StringContent CreateSoapContent(string soapEnvelope, string soapActionUrl)
        {
            StringContent content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

            // Add SOAPAction header
            if (!string.IsNullOrWhiteSpace(soapActionUrl))
            {
                content.Headers.Remove("SOAPAction");
                content.Headers.Add("SOAPAction", soapActionUrl);
            }

            return content;
        }
    }
}
