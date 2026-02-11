using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace SystemLayer.Infrastructure.Soap;

/// <summary>
/// Builder for SOAP messages with proper envelope structure
/// </summary>
public class SoapMessageBuilder
{
    private readonly ILogger<SoapMessageBuilder> _logger;
    private readonly string _soapNamespace;
    
    public SoapMessageBuilder(ILogger<SoapMessageBuilder> logger, string soapNamespace = "http://cafm.mri.com/services")
    {
        _logger = logger;
        _soapNamespace = soapNamespace;
    }
    
    /// <summary>
    /// Builds a complete SOAP envelope with the given body content
    /// </summary>
    /// <param name="soapAction">SOAP action for the operation</param>
    /// <param name="bodyContent">XML content for the SOAP body</param>
    /// <param name="correlationId">Correlation ID for logging</param>
    /// <returns>Complete SOAP envelope as XML string</returns>
    public string BuildSoapEnvelope(string soapAction, string bodyContent, string correlationId)
    {
        try
        {
            var soapEnvelope = new StringBuilder();
            soapEnvelope.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soapEnvelope.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"" + _soapNamespace + "\">");
            
            // SOAP Header with correlation ID
            soapEnvelope.AppendLine("  <soap:Header>");
            soapEnvelope.AppendLine($"    <tns:CorrelationId>{correlationId}</tns:CorrelationId>");
            soapEnvelope.AppendLine("  </soap:Header>");
            
            // SOAP Body
            soapEnvelope.AppendLine("  <soap:Body>");
            soapEnvelope.AppendLine($"    <tns:{soapAction}>");
            soapEnvelope.AppendLine(bodyContent);
            soapEnvelope.AppendLine($"    </tns:{soapAction}>");
            soapEnvelope.AppendLine("  </soap:Body>");
            soapEnvelope.AppendLine("</soap:Envelope>");
            
            var result = soapEnvelope.ToString();
            
            _logger.LogDebug("Built SOAP envelope for action {SoapAction} with correlation {CorrelationId}", 
                soapAction, correlationId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build SOAP envelope for action {SoapAction} with correlation {CorrelationId}", 
                soapAction, correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Serializes an object to XML for use in SOAP body
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <param name="correlationId">Correlation ID for logging</param>
    /// <returns>XML representation of the object</returns>
    public string SerializeToXml<T>(T obj, string correlationId) where T : class
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                Encoding = Encoding.UTF8
            };
            
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", _soapNamespace);
            
            serializer.Serialize(xmlWriter, obj, namespaces);
            
            var result = stringWriter.ToString();
            
            _logger.LogDebug("Serialized {ObjectType} to XML with correlation {CorrelationId}", 
                typeof(T).Name, correlationId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize {ObjectType} to XML with correlation {CorrelationId}", 
                typeof(T).Name, correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Deserializes XML response to strongly typed object
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="xml">XML content to deserialize</param>
    /// <param name="correlationId">Correlation ID for logging</param>
    /// <returns>Deserialized object</returns>
    public T DeserializeFromXml<T>(string xml, string correlationId) where T : class
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader);
            
            var result = (T)serializer.Deserialize(xmlReader)!;
            
            _logger.LogDebug("Deserialized XML to {ObjectType} with correlation {CorrelationId}", 
                typeof(T).Name, correlationId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize XML to {ObjectType} with correlation {CorrelationId}. XML: {Xml}", 
                typeof(T).Name, correlationId, xml);
            throw;
        }
    }
    
    /// <summary>
    /// Extracts SOAP body content from a SOAP envelope
    /// </summary>
    /// <param name="soapEnvelope">Complete SOAP envelope</param>
    /// <param name="correlationId">Correlation ID for logging</param>
    /// <returns>SOAP body content</returns>
    public string ExtractSoapBody(string soapEnvelope, string correlationId)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapEnvelope);
            
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            
            var bodyNode = doc.SelectSingleNode("//soap:Body", namespaceManager);
            if (bodyNode?.FirstChild != null)
            {
                return bodyNode.FirstChild.OuterXml;
            }
            
            _logger.LogWarning("No SOAP body found in envelope with correlation {CorrelationId}", correlationId);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract SOAP body with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Checks if the SOAP response contains a fault
    /// </summary>
    /// <param name="soapEnvelope">SOAP envelope to check</param>
    /// <param name="correlationId">Correlation ID for logging</param>
    /// <returns>True if fault is present, false otherwise</returns>
    public bool ContainsSoapFault(string soapEnvelope, string correlationId)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapEnvelope);
            
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            
            var faultNode = doc.SelectSingleNode("//soap:Fault", namespaceManager);
            var hasFault = faultNode != null;
            
            if (hasFault)
            {
                _logger.LogWarning("SOAP fault detected in response with correlation {CorrelationId}", correlationId);
            }
            
            return hasFault;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for SOAP fault with correlation {CorrelationId}", correlationId);
            return false;
        }
    }
    
    /// <summary>
    /// Extracts SOAP fault information from response
    /// </summary>
    /// <param name="soapEnvelope">SOAP envelope containing fault</param>
    /// <param name="correlationId">Correlation ID for logging</param>
    /// <returns>SOAP fault details</returns>
    public (string FaultCode, string FaultString, string? Detail) ExtractSoapFault(string soapEnvelope, string correlationId)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapEnvelope);
            
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            
            var faultNode = doc.SelectSingleNode("//soap:Fault", namespaceManager);
            if (faultNode != null)
            {
                var faultCode = faultNode.SelectSingleNode("faultcode")?.InnerText ?? "Unknown";
                var faultString = faultNode.SelectSingleNode("faultstring")?.InnerText ?? "Unknown error";
                var detail = faultNode.SelectSingleNode("detail")?.InnerText;
                
                _logger.LogError("SOAP fault extracted - Code: {FaultCode}, String: {FaultString}, Detail: {Detail}, Correlation: {CorrelationId}", 
                    faultCode, faultString, detail, correlationId);
                
                return (faultCode, faultString, detail);
            }
            
            return ("NoFault", "No fault found", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract SOAP fault with correlation {CorrelationId}", correlationId);
            return ("ExtractionError", "Failed to extract fault details", ex.Message);
        }
    }
}