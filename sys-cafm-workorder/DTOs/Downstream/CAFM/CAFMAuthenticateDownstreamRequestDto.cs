using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream request DTO for CAFM Authenticate SOAP call.
/// </summary>
public class CAFMAuthenticateDownstreamRequestDto : IDownStreamRequestDTO
{
    /// <summary>
    /// Login name for CAFM authentication.
    /// </summary>
    public string LoginName { get; set; } = string.Empty;

    /// <summary>
    /// Password for CAFM authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

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

        if (string.IsNullOrWhiteSpace(LoginName))
            errors.Add("LoginName is required for CAFM authentication.");

        if (string.IsNullOrWhiteSpace(Password))
            errors.Add("Password is required for CAFM authentication.");

        if (string.IsNullOrWhiteSpace(Url))
            errors.Add("Url is required for CAFM authentication.");

        if (string.IsNullOrWhiteSpace(SoapAction))
            errors.Add("SoapAction is required for CAFM authentication.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CAFMAuthenticateDownstreamRequestDto));
        }
    }

    /// <summary>
    /// Generates the SOAP XML envelope for the Authenticate request.
    /// </summary>
    public string ToSoapXml()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:Authenticate>
         <ns:loginName>{EscapeXml(LoginName)}</ns:loginName>
         <ns:password>{EscapeXml(Password)}</ns:password>
      </ns:Authenticate>
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
