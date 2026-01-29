using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CAFMSystem.Helper
{
    /// <summary>
    /// Static utility class for SOAP envelope operations.
    /// Loads templates, deserializes responses, provides helper methods.
    /// </summary>
    public static class SOAPHelper
    {
        /// <summary>
        /// Loads SOAP envelope template from embedded resources.
        /// </summary>
        /// <param name="resourceName">Full resource name (e.g., "CAFMSystem.SoapEnvelopes.Authenticate.xml")</param>
        /// <returns>SOAP envelope template as string</returns>
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

        /// <summary>
        /// Returns SOAP element if value is not empty, otherwise returns empty string.
        /// </summary>
        /// <param name="elementName">Element name (e.g., "ns:Priority")</param>
        /// <param name="value">Element value</param>
        /// <returns>Full element or empty string</returns>
        public static string GetElementOrEmpty(string elementName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return $"<{elementName}>{value}</{elementName}>";
        }

        /// <summary>
        /// Returns value or empty string if null/whitespace.
        /// </summary>
        public static string GetValueOrEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        /// <summary>
        /// Deserializes SOAP XML response to typed DTO.
        /// Extracts content from SOAP envelope and deserializes to object.
        /// </summary>
        /// <typeparam name="TResponse">Response DTO type</typeparam>
        /// <param name="xmlContent">SOAP XML response</param>
        /// <param name="rootElementName">Root element name to extract (optional)</param>
        /// <returns>Deserialized response DTO</returns>
        public static TResponse? DeserializeSoapResponse<TResponse>(string xmlContent, string? rootElementName = null) where TResponse : class
        {
            ILogger<object> logger = ServiceLocator.GetRequiredService<ILogger<object>>();
            logger.Info($"Deserializing SOAP response to {typeof(TResponse).Name}");

            try
            {
                // Parse XML
                XDocument xdoc = XDocument.Parse(xmlContent);

                // Extract Body content (skip SOAP envelope)
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XElement? body = xdoc.Root?.Element(soapNs + "Body");

                if (body == null)
                {
                    logger.Warn("SOAP Body element not found in response");
                    return null;
                }

                // Get first child element in Body (the actual response)
                XElement? responseElement = body.FirstNode as XElement;

                if (responseElement == null)
                {
                    logger.Warn("No response element found in SOAP Body");
                    return null;
                }

                // Serialize to JSON for deserialization
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    // Extract properties from XML element
                    // This is a simplified approach - adjust based on actual SOAP response structure
                });

                // For SOAP responses, we'll use XML deserialization
                using StringReader stringReader = new StringReader(responseElement.ToString());
                XmlSerializer serializer = new XmlSerializer(typeof(TResponse));
                return serializer.Deserialize(stringReader) as TResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to deserialize SOAP response: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Extracts text value from XML element by name.
        /// </summary>
        public static string ExtractElementValue(string xmlContent, string elementName)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlContent);
                XElement? element = xdoc.Descendants().FirstOrDefault(e => e.Name.LocalName == elementName);
                return element?.Value ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
