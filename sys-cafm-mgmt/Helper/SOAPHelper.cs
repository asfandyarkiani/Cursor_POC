using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json;
using System.Xml;

namespace sys_cafm_mgmt.Helper
{
    public static class SOAPHelper
    {
        public static string LoadSoapEnvelopeTemplate(string resourceName)
        {
            ILogger<object> logger = ServiceLocator.GetRequiredService<ILogger<object>>();
            logger.Info($"Loading SOAP envelope template: {resourceName}");
            
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
            ILogger<object> logger = ServiceLocator.GetRequiredService<ILogger<object>>();
            logger.Info($"Deserializing SOAP response to {typeof(TResponse).Name}");
            
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);
            
            string jsonContent = JsonConvert.SerializeXmlNode(xmlDocument);
            
            return System.Text.Json.JsonSerializer.Deserialize<TResponse>(jsonContent, jsonOptions);
        }
    }
}
