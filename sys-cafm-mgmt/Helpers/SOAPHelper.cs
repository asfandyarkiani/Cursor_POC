using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json;
using System.Xml;

namespace CAFMSystem.Helpers
{
    /// <summary>
    /// Static utility for SOAP envelope operations.
    /// Handles template loading, element generation, and response deserialization.
    /// </summary>
    public static class SOAPHelper
    {
        /// <summary>
        /// Loads SOAP envelope template from embedded resources.
        /// </summary>
        /// <param name="resourceName">Full resource name (e.g., CAFMSystem.SoapEnvelopes.Authenticate.xml)</param>
        /// <returns>SOAP envelope template string</returns>
        public static string LoadSoapEnvelopeTemplate(string resourceName)
        {
            ILogger<SOAPHelper> logger = ServiceLocator.GetRequiredService<ILogger<SOAPHelper>>();
            logger.Info($"Loading SOAP envelope template: {resourceName}");
            
            Assembly assembly = typeof(SOAPHelper).Assembly;
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            
            if (stream == null)
                throw new FileNotFoundException($"SOAP template not found: {resourceName}");
            
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Generates SOAP element with value or returns empty string if value is null/empty.
        /// </summary>
        /// <param name="elementName">Element name (e.g., "ns:Priority")</param>
        /// <param name="value">Element value</param>
        /// <returns>SOAP element string or empty string</returns>
        public static string GetSoapElement(string elementName, string? value)
        {
            return string.IsNullOrWhiteSpace(value) 
                ? string.Empty 
                : $"<{elementName}>{value}</{elementName}>";
        }

        /// <summary>
        /// Returns value or empty string if null/whitespace.
        /// </summary>
        public static string GetValueOrEmpty(string? value)
        {
            return value ?? string.Empty;
        }

        /// <summary>
        /// Generates SOAP element only if value is not null/empty, otherwise returns empty string.
        /// </summary>
        public static string GetElementOrEmpty(string elementName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            
            return $"<{elementName}>{value}</{elementName}>";
        }

        /// <summary>
        /// Deserializes SOAP XML response to DTO using XML to JSON conversion.
        /// </summary>
        /// <typeparam name="TResponse">Response DTO type</typeparam>
        /// <param name="xmlContent">SOAP XML response</param>
        /// <param name="jsonOptions">Optional JSON serializer options</param>
        /// <returns>Deserialized response DTO</returns>
        public static TResponse? DeserializeSoapResponse<TResponse>(
            string xmlContent,
            JsonSerializerOptions? jsonOptions = null)
        {
            ILogger<SOAPHelper> logger = ServiceLocator.GetRequiredService<ILogger<SOAPHelper>>();
            logger.Info($"Deserializing SOAP response to {typeof(TResponse).Name}");
            
            string json = ConvertXmlToJson(xmlContent);
            
            JsonSerializerOptions options = jsonOptions ?? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            return System.Text.Json.JsonSerializer.Deserialize<TResponse>(json, options);
        }

        /// <summary>
        /// Converts XML to JSON for easier deserialization.
        /// </summary>
        private static string ConvertXmlToJson(string xmlContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            return JsonConvert.SerializeXmlNode(doc);
        }
    }
}
