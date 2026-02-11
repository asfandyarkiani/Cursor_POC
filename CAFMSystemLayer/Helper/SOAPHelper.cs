using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace CAFMSystemLayer.Helper
{
    public static class SOAPHelper
    {
        /// <summary>
        /// Loads SOAP envelope template from embedded resources.
        /// </summary>
        /// <param name="resourceName">Full resource name: ProjectNamespace.SoapEnvelopes.FileName.xml</param>
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
        /// Returns SOAP element if value is not null/empty, otherwise returns empty string.
        /// </summary>
        public static string GetElementOrEmpty(string elementName, string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : $"<{elementName}>{value}</{elementName}>";
        }
        
        /// <summary>
        /// Returns value or empty string if null.
        /// </summary>
        public static string GetValueOrEmpty(string? value)
        {
            return value ?? string.Empty;
        }
        
        /// <summary>
        /// Deserializes SOAP XML response to DTO.
        /// </summary>
        public static TResponse? DeserializeSoapResponse<TResponse>(string xmlContent)
        {
            ILogger<SOAPHelper> logger = ServiceLocator.GetRequiredService<ILogger<SOAPHelper>>();
            logger.Info($"Deserializing SOAP response to {typeof(TResponse).Name}");
            
            try
            {
                // Parse XML and extract body content
                XDocument doc = XDocument.Parse(xmlContent);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                
                XElement? body = doc.Root?.Element(soapNs + "Body");
                if (body == null)
                {
                    logger.Warn("SOAP Body element not found in response");
                    return default;
                }
                
                // Convert first child element to JSON for deserialization
                XElement? responseElement = body.Elements().FirstOrDefault();
                if (responseElement == null)
                {
                    logger.Warn("No response element found in SOAP Body");
                    return default;
                }
                
                // Simple XML to object mapping
                string json = System.Text.Json.JsonSerializer.Serialize(
                    ConvertXElementToObject(responseElement)
                );
                
                return JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to deserialize SOAP response");
                throw;
            }
        }
        
        private static object ConvertXElementToObject(XElement element)
        {
            if (!element.HasElements)
            {
                return element.Value;
            }
            
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (XElement child in element.Elements())
            {
                string key = child.Name.LocalName;
                object value = ConvertXElementToObject(child);
                
                if (dict.ContainsKey(key))
                {
                    // Handle multiple elements with same name (convert to list)
                    if (dict[key] is List<object> list)
                    {
                        list.Add(value);
                    }
                    else
                    {
                        dict[key] = new List<object> { dict[key], value };
                    }
                }
                else
                {
                    dict[key] = value;
                }
            }
            
            return dict;
        }
    }
}
