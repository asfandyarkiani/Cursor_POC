using Core.Extensions;
using Core.Headers;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace sys_cafm_mgmt.Helper
{
    public class CustomSoapClient
    {
        private readonly CustomHTTPClient _customHTTPClient;
        private readonly ILogger<CustomSoapClient> _logger;

        public CustomSoapClient(CustomHTTPClient customHTTPClient, ILogger<CustomSoapClient> logger)
        {
            _customHTTPClient = customHTTPClient;
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
            HttpResponseMessage httpResponseMessage = await _customHTTPClient.SendAsync(
                httpMethod: httpMethod,
                url: apiUrl ?? string.Empty,
                contentFactory: () => soapContent,
                headers: null);
            
            HttpResponseSnapshot httpResponseSnapshot = await HttpResponseSnapshot.FromAsync(httpResponseMessage);
            
            stopwatch.Stop();
            ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{stopwatch.ElapsedMilliseconds},");
            
            _logger.Info($"SOAP request completed: {operationName} - Status: {httpResponseSnapshot.StatusCode}");
            
            return httpResponseSnapshot;
        }

        public static StringContent CreateSoapContent(string soapEnvelope, string? soapActionUrl)
        {
            StringContent stringContent = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            
            if (!string.IsNullOrWhiteSpace(soapActionUrl))
            {
                stringContent.Headers.Add("SOAPAction", soapActionUrl);
            }
            
            return stringContent;
        }
    }
}
