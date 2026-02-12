using System.Xml;
using System.Xml.Serialization;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Helper class for SOAP operations.
/// </summary>
public static class SOAPHelper
{
    /// <summary>
    /// Deserializes a SOAP response body to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="soapResponse">The SOAP response XML string.</param>
    /// <returns>The deserialized object.</returns>
    public static T? DeserializeSoapResponse<T>(string soapResponse) where T : class
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapResponse);

            // Find the SOAP body
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");

            var body = doc.SelectSingleNode("//soap:Body", namespaceManager)
                ?? doc.SelectSingleNode("//soap12:Body", namespaceManager);

            if (body == null || body.FirstChild == null)
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(body.FirstChild.OuterXml);
            return serializer.Deserialize(reader) as T;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the fault message from a SOAP fault response.
    /// </summary>
    /// <param name="soapResponse">The SOAP response XML string.</param>
    /// <returns>The fault message if present, otherwise null.</returns>
    public static string? ExtractSoapFault(string soapResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapResponse);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");

            var fault = doc.SelectSingleNode("//soap:Fault", namespaceManager)
                ?? doc.SelectSingleNode("//soap12:Fault", namespaceManager);

            if (fault != null)
            {
                var faultString = fault.SelectSingleNode("faultstring")?.InnerText
                    ?? fault.SelectSingleNode("soap12:Reason/soap12:Text", namespaceManager)?.InnerText;
                return faultString;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
