using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream request DTO for CAFM GetBreakdownTasksByDto SOAP call.
/// </summary>
public class CAFMGetBreakdownTasksDownstreamRequestDto : IDownStreamRequestDTO
{
    /// <summary>
    /// Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Caller source ID to filter breakdown tasks.
    /// </summary>
    public string CallerSourceId { get; set; } = string.Empty;

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
            errors.Add("SessionId is required.");

        if (string.IsNullOrWhiteSpace(CallerSourceId))
            errors.Add("CallerSourceId is required.");

        if (string.IsNullOrWhiteSpace(Url))
            errors.Add("Url is required.");

        if (string.IsNullOrWhiteSpace(SoapAction))
            errors.Add("SoapAction is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CAFMGetBreakdownTasksDownstreamRequestDto));
        }
    }

    /// <summary>
    /// Generates the SOAP XML envelope for the GetBreakdownTasksByDto request.
    /// </summary>
    public string ToSoapXml()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:GetBreakdownTasksByDto>
         <ns:sessionId>{EscapeXml(SessionId)}</ns:sessionId>
         <ns:breakdownTaskDto>
            <ns:BDET_CALLER_SOURCE_ID>{EscapeXml(CallerSourceId)}</ns:BDET_CALLER_SOURCE_ID>
         </ns:breakdownTaskDto>
      </ns:GetBreakdownTasksByDto>
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
