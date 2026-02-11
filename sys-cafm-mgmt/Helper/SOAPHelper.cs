using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using Newtonsoft.Json;

namespace CAFMSystem.Helper
{
    public static class SOAPHelper
    {
        public static string LoadSoapEnvelopeTemplate(string resourceName)
        {
            ILogger<SOAPHelper>? logger = ServiceLocator.ServiceProvider?.GetService<ILogger<SOAPHelper>>();
            logger?.Info($"Loading SOAP envelope template: {resourceName}");
            
            Assembly assembly = typeof(SOAPHelper).Assembly;
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            
            if (stream == null)
                throw new FileNotFoundException($"SOAP template not found: {resourceName}");
            
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static string GetSoapElement(string elementName, string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : $"<{elementName}>{value}</{elementName}>";
        }

        public static string GetValueOrEmpty(string? value)
        {
            return value ?? string.Empty;
        }

        public static string GetElementOrEmpty(string elementName, string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : $"<{elementName}>{value}</{elementName}>";
        }

        public static TResponse? DeserializeSoapResponse<TResponse>(string xmlContent, JsonSerializerOptions? jsonOptions = null)
        {
            string json = ConvertXmlToJson(xmlContent);
            return System.Text.Json.JsonSerializer.Deserialize<TResponse>(json, jsonOptions);
        }

        private static string ConvertXmlToJson(string xmlContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            return JsonConvert.SerializeXmlNode(doc);
        }
    }
}
