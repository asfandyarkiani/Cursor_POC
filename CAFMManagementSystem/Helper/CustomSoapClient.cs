using Core.Extensions;
using Core.Headers;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace CAFMManagementSystem.Helper
{
    public class CustomSoapClient
    {
        private readonly CustomHTTPClient _customHttpClient;
        private readonly ILogger<CustomSoapClient> _logger;
        
        public CustomSoapClient(CustomHTTPClient customHttpClient, ILogger<CustomSoapClient> logger)
        {
            _customHttpClient = customHttpClient;
            _logger = logger;
        }
        
        public async Task<HttpResponseSnapshot> ExecuteCustomSoapRequestAsync(
            string operationName,
            string soapEnvelope,
            string? apiUrl,
            string? soapActionUrl,
            HttpMethod httpMethod)
        {
            _logger.Info($"Executing SOAP request: {operationName}");
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            StringContent soapContent = CreateSoapContent(soapEnvelope, soapActionUrl);
            
            List<Tuple<string, string>> headers = new List<Tuple<string, string>>();
            
            HttpResponseMessage response = await _customHttpClient.SendAsync(
                httpMethod,
                apiUrl ?? string.Empty,
                () => soapContent,
                headers
            );
            
            HttpResponseSnapshot result = await HttpResponseSnapshot.FromAsync(response);
            
            stopwatch.Stop();
            ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{stopwatch.ElapsedMilliseconds},");
            
            _logger.Info($"SOAP request completed: {operationName} - Status: {result.StatusCode}");
            
            return result;
        }
        
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
