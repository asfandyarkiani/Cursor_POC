using System.Xml.Linq;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream response DTO for CAFM CreateEvent SOAP call.
/// </summary>
public class CAFMCreateEventDownstreamResponseDto
{
    /// <summary>
    /// Indicates whether the event was created successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Raw XML response for debugging.
    /// </summary>
    public string RawResponse { get; set; } = string.Empty;

    /// <summary>
    /// Parses the SOAP XML response to determine success.
    /// </summary>
    public static CAFMCreateEventDownstreamResponseDto FromSoapXml(string xml)
    {
        var response = new CAFMCreateEventDownstreamResponseDto { RawResponse = xml };

        if (string.IsNullOrWhiteSpace(xml))
            return response;

        try
        {
            var doc = XDocument.Parse(xml);
            
            // Check if response contains a fault
            var faultElement = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
            response.IsSuccess = faultElement == null;
        }
        catch
        {
            // If parsing fails, assume failure
            response.IsSuccess = false;
        }

        return response;
    }
}
