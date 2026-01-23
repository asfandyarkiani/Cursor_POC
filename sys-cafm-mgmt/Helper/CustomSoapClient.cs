using Core.Headers;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Headers;

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
            Stopwatch sw = Stopwatch.StartNew();
            
            StringContent content = CreateSoapContent(soapEnvelope, soapActionUrl);
            
            HttpResponseMessage response = await _customHTTPClient.SendAsync(
                httpMethod,
                apiUrl,
                () => content,
                new List<Tuple<string, string>>());
            
            HttpResponseSnapshot result = await HttpResponseSnapshot.FromAsync(response);
            
            sw.Stop();
            ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{sw.ElapsedMilliseconds},");
            
            return result;
        }

        public static StringContent CreateSoapContent(string soapEnvelope, string soapActionUrl)
        {
            StringContent content = new StringContent(soapEnvelope, System.Text.Encoding.UTF8, "text/xml");
            content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            
            if (!string.IsNullOrWhiteSpace(soapActionUrl))
            {
                content.Headers.Add("SOAPAction", soapActionUrl);
            }
            
            return content;
        }
    }
}
