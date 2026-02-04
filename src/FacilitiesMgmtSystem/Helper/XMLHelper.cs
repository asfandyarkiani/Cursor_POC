using System.Xml;
using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Helper class for XML operations.
/// </summary>
public static class XMLHelper
{
    /// <summary>
    /// Serializes an object to XML string.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The XML string representation.</returns>
    public static string Serialize<T>(T obj) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
        using var writer = new StringWriter();
        serializer.Serialize(writer, obj);
        return writer.ToString();
    }

    /// <summary>
    /// Deserializes an XML string to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="xml">The XML string.</param>
    /// <returns>The deserialized object or null if deserialization fails.</returns>
    public static T? Deserialize<T>(string xml) where T : class
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xml);
            return serializer.Deserialize(reader) as T;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the value of an XML element by path.
    /// </summary>
    /// <param name="xml">The XML string.</param>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>The element value or null.</returns>
    public static string? GetElementValue(string xml, string xpath)
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
    /// Sets the value of an XML element by path.
    /// </summary>
    /// <param name="xml">The XML string.</param>
    /// <param name="xpath">The XPath expression.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The modified XML string.</returns>
    public static string SetElementValue(string xml, string xpath, string value)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var node = doc.SelectSingleNode(xpath);
        if (node != null)
        {
            node.InnerText = value;
        }
        return doc.OuterXml;
    }
}
