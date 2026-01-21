using System.Xml.Linq;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream response DTO for CAFM Authenticate SOAP call.
/// </summary>
public class CAFMAuthenticateDownstreamResponseDto
{
    /// <summary>
    /// Session ID returned from CAFM.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether authentication was successful.
    /// </summary>
    public bool IsSuccess => !string.IsNullOrWhiteSpace(SessionId);

    /// <summary>
    /// Raw XML response for debugging.
    /// </summary>
    public string RawResponse { get; set; } = string.Empty;

    /// <summary>
    /// Parses the SOAP XML response to extract the SessionId.
    /// </summary>
    public static CAFMAuthenticateDownstreamResponseDto FromSoapXml(string xml)
    {
        var response = new CAFMAuthenticateDownstreamResponseDto { RawResponse = xml };

        if (string.IsNullOrWhiteSpace(xml))
            return response;

        try
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

            var sessionIdElement = doc.Descendants(ns + "SessionId").FirstOrDefault()
                ?? doc.Descendants("SessionId").FirstOrDefault();

            if (sessionIdElement != null)
            {
                response.SessionId = sessionIdElement.Value;
            }
        }
        catch
        {
            // If parsing fails, return empty response
        }

        return response;
    }
}
