using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Helper class for XML operations.
/// </summary>
public static class XMLHelper
{
    /// <summary>
    /// Serializes an object to an XML string.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="omitXmlDeclaration">Whether to omit the XML declaration.</param>
    /// <returns>The XML string representation.</returns>
    public static string Serialize<T>(T obj, bool omitXmlDeclaration = true)
    {
        var serializer = new XmlSerializer(typeof(T));
        
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = omitXmlDeclaration,
            Indent = false,
            Encoding = Encoding.UTF8
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        
        // Create empty namespaces to avoid xmlns:xsi and xmlns:xsd
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("", "");
        
        serializer.Serialize(xmlWriter, obj, namespaces);
        return stringWriter.ToString();
    }

    /// <summary>
    /// Deserializes an XML string to an object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="xml">The XML string.</param>
    /// <returns>The deserialized object.</returns>
    public static T? Deserialize<T>(string xml) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stringReader = new StringReader(xml);
        return serializer.Deserialize(stringReader) as T;
    }

    /// <summary>
    /// Escapes special XML characters in a string.
    /// </summary>
    /// <param name="value">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    public static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    /// <summary>
    /// Extracts a node value from XML by XPath.
    /// </summary>
    /// <param name="xml">The XML string.</param>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>The node value or null if not found.</returns>
    public static string? GetNodeValue(string xml, string xpath)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var node = doc.SelectSingleNode(xpath);
            return node?.InnerText;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Replaces placeholders in an XML template with values from a dictionary.
    /// </summary>
    /// <param name="template">The XML template with placeholders (e.g., {{PlaceholderName}}).</param>
    /// <param name="values">Dictionary of placeholder names and their values.</param>
    /// <returns>The XML string with placeholders replaced.</returns>
    public static string ReplacePlaceholders(string template, Dictionary<string, string?> values)
    {
        var result = template;
        foreach (var kvp in values)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            var value = EscapeXml(kvp.Value);
            result = result.Replace(placeholder, value);
        }
        return result;
    }
}
