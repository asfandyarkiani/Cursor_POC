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
            string apiUrl,
            string soapActionUrl,
            HttpMethod httpMethod)
        {
            _logger.Info($"Executing SOAP request: {operationName}");
            
            Stopwatch sw = Stopwatch.StartNew();
            
            HttpResponseMessage response = await ExecuteSoapRequestAsync(
                soapEnvelope, 
                apiUrl, 
                soapActionUrl, 
                httpMethod);
                
            HttpResponseSnapshot result = await HttpResponseSnapshot.FromAsync(response);
            
            sw.Stop();
            ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{sw.ElapsedMilliseconds},");
            
            _logger.Info($"SOAP request completed: {operationName} - Status: {result.StatusCode}");
            
            return result;
        }

        private async Task<HttpResponseMessage> ExecuteSoapRequestAsync(
            string soapEnvelope,
            string apiUrl,
            string soapActionUrl,
            HttpMethod httpMethod)
        {
            StringContent content = CreateSoapContent(soapEnvelope, soapActionUrl);
            
            return await _customHTTPClient.SendAsync(
                httpMethod,
                apiUrl,
                () => content,
                headers: null);
        }

        public static StringContent CreateSoapContent(string soapEnvelope, string soapActionUrl)
        {
            StringContent content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            
            if (!string.IsNullOrWhiteSpace(soapActionUrl))
            {
                content.Headers.Remove("SOAPAction");
                content.Headers.Add("SOAPAction", soapActionUrl);
            }
            
            return content;
        }
    }
}
