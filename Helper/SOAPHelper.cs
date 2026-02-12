using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Helper class for SOAP operations including envelope loading and response deserialization.
/// </summary>
public static class SOAPHelper
{
    /// <summary>
    /// Loads a SOAP envelope template from embedded resources.
    /// </summary>
    /// <param name="resourceName">The full resource name (e.g., FacilitiesMgmtSystem.SoapEnvelopes.CreateWorkOrder.xml)</param>
    /// <returns>The SOAP envelope template as a string.</returns>
    public static string LoadSoapEnvelope(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new BaseException(string.Format(ErrorConstants.SOAP_ENVELOPE_NOT_FOUND, resourceName));
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Deserializes a SOAP response XML to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="soapResponse">The full SOAP response XML.</param>
    /// <param name="bodyElementName">The name of the body element to extract.</param>
    /// <param name="namespaceUri">Optional namespace URI for the body element.</param>
    /// <returns>The deserialized object.</returns>
    public static T? DeserializeSoapResponse<T>(string soapResponse, string bodyElementName, string? namespaceUri = null) 
        where T : class
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapResponse);

            // Create namespace manager for SOAP namespaces
            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");
            
            if (!string.IsNullOrEmpty(namespaceUri))
            {
                nsManager.AddNamespace("ns", namespaceUri);
            }

            // Try to find the body element
            XmlNode? bodyContent = null;
            
            // Try SOAP 1.1
            bodyContent = doc.SelectSingleNode($"//soap:Body/{bodyElementName}", nsManager);
            
            // Try SOAP 1.2 if 1.1 didn't work
            if (bodyContent == null)
            {
                bodyContent = doc.SelectSingleNode($"//soap12:Body/{bodyElementName}", nsManager);
            }

            // Try with namespace prefix if provided
            if (bodyContent == null && !string.IsNullOrEmpty(namespaceUri))
            {
                bodyContent = doc.SelectSingleNode($"//soap:Body/ns:{bodyElementName}", nsManager);
                if (bodyContent == null)
                {
                    bodyContent = doc.SelectSingleNode($"//soap12:Body/ns:{bodyElementName}", nsManager);
                }
            }

            // Try without namespace
            if (bodyContent == null)
            {
                bodyContent = doc.SelectSingleNode($"//*[local-name()='{bodyElementName}']");
            }

            if (bodyContent == null)
            {
                throw new DownStreamApiFailureException(
                    $"Could not find element '{bodyElementName}' in SOAP response.");
            }

            // Deserialize the content
            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(bodyContent.OuterXml);
            var result = serializer.Deserialize(stringReader) as T;
            
            return result;
        }
        catch (XmlException ex)
        {
            throw new DownStreamApiFailureException(
                ErrorConstants.SOAP_RESPONSE_DESERIALIZATION_FAILED, ex);
        }
    }

    /// <summary>
    /// Extracts a simple value from an XML string by element name.
    /// </summary>
    /// <param name="xml">The XML string.</param>
    /// <param name="elementName">The element name to extract.</param>
    /// <returns>The element value or null if not found.</returns>
    public static string? ExtractValueFromXml(string xml, string elementName)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var node = doc.SelectSingleNode($"//*[local-name()='{elementName}']");
            return node?.InnerText;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a SOAP response contains a fault.
    /// </summary>
    /// <param name="soapResponse">The SOAP response XML.</param>
    /// <returns>True if the response contains a SOAP fault.</returns>
    public static bool ContainsSoapFault(string soapResponse)
    {
        return soapResponse.Contains("<soap:Fault") || 
               soapResponse.Contains("<Fault") ||
               soapResponse.Contains(":Fault>");
    }

    /// <summary>
    /// Extracts the fault message from a SOAP fault response.
    /// </summary>
    /// <param name="soapResponse">The SOAP response XML.</param>
    /// <returns>The fault message or a default message if not found.</returns>
    public static string ExtractSoapFaultMessage(string soapResponse)
    {
        var faultString = ExtractValueFromXml(soapResponse, "faultstring");
        if (!string.IsNullOrEmpty(faultString))
        {
            return faultString;
        }

        var reason = ExtractValueFromXml(soapResponse, "Reason");
        if (!string.IsNullOrEmpty(reason))
        {
            return reason;
        }

        return ErrorConstants.SOAP_FAULT_RECEIVED;
    }
}
