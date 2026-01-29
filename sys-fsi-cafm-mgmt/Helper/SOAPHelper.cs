using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace FsiCafmSystem.Helper
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
            {
                throw new FileNotFoundException($"SOAP template not found: {resourceName}");
            }
            
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
            ILogger<SOAPHelper>? logger = ServiceLocator.ServiceProvider?.GetService<ILogger<SOAPHelper>>();
            logger?.Info($"Deserializing SOAP response to {typeof(TResponse).Name}");
            
            try
            {
                // Parse XML and extract the relevant data
                XDocument doc = XDocument.Parse(xmlContent);
                
                // Remove namespaces for easier parsing
                foreach (XElement element in doc.Descendants())
                {
                    element.Name = element.Name.LocalName;
                    element.ReplaceAttributes(element.Attributes().Where(a => !a.IsNamespaceDeclaration));
                }
                
                // Convert to JSON
                string json = System.Text.Json.JsonSerializer.Serialize(XmlToJson(doc.Root!));
                
                // Deserialize to target type
                JsonSerializerOptions options = jsonOptions ?? new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<TResponse>(json, options);
            }
            catch (Exception ex)
            {
                logger?.Error(ex, $"Failed to deserialize SOAP response to {typeof(TResponse).Name}");
                throw;
            }
        }
        
        private static Dictionary<string, object> XmlToJson(XElement element)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            
            foreach (XElement child in element.Elements())
            {
                if (child.HasElements)
                {
                    if (dict.ContainsKey(child.Name.LocalName))
                    {
                        // Handle arrays
                        if (dict[child.Name.LocalName] is not List<Dictionary<string, object>> list)
                        {
                            object existing = dict[child.Name.LocalName];
                            list = new List<Dictionary<string, object>>();
                            if (existing is Dictionary<string, object> existingDict)
                            {
                                list.Add(existingDict);
                            }
                            dict[child.Name.LocalName] = list;
                        }
                        list.Add(XmlToJson(child));
                    }
                    else
                    {
                        dict[child.Name.LocalName] = XmlToJson(child);
                    }
                }
                else
                {
                    dict[child.Name.LocalName] = child.Value;
                }
            }
            
            return dict;
        }
    }
}
