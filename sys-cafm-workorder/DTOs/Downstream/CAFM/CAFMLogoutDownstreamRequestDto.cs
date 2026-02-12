using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream request DTO for CAFM Logout SOAP call.
/// </summary>
public class CAFMLogoutDownstreamRequestDto : IDownStreamRequestDTO
{
    /// <summary>
    /// Session ID to terminate.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Full URL for the SOAP endpoint.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// SOAP Action header value.
    /// </summary>
    public string SoapAction { get; set; } = string.Empty;

    public void ValidateDownStreamRequestParameters()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SessionId))
            errors.Add("SessionId is required for CAFM logout.");

        if (string.IsNullOrWhiteSpace(Url))
            errors.Add("Url is required for CAFM logout.");

        if (string.IsNullOrWhiteSpace(SoapAction))
            errors.Add("SoapAction is required for CAFM logout.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CAFMLogoutDownstreamRequestDto));
        }
    }

    /// <summary>
    /// Generates the SOAP XML envelope for the Logout request.
    /// </summary>
    public string ToSoapXml()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:Logout>
         <ns:sessionId>{EscapeXml(SessionId)}</ns:sessionId>
      </ns:Logout>
   </soapenv:Body>
</soapenv:Envelope>";
    }

    private static string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
