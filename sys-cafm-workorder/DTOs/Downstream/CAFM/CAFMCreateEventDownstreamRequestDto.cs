using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream request DTO for CAFM CreateEvent SOAP call.
/// </summary>
public class CAFMCreateEventDownstreamRequestDto : IDownStreamRequestDTO
{
    /// <summary>
    /// Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Task ID to link the event to.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Event type code.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Event description or notes.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date of the event (UTC) - formatted for CAFM.
    /// </summary>
    public string EventDateUtc { get; set; } = string.Empty;

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

        if (string.IsNullOrWhiteSpace(TaskId))
            errors.Add("TaskId is required.");

        if (string.IsNullOrWhiteSpace(Url))
            errors.Add("Url is required.");

        if (string.IsNullOrWhiteSpace(SoapAction))
            errors.Add("SoapAction is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CAFMCreateEventDownstreamRequestDto));
        }
    }

    /// <summary>
    /// Generates the SOAP XML envelope for the CreateEvent request.
    /// </summary>
    public string ToSoapXml()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:CreateEvent>
         <ns:sessionId>{EscapeXml(SessionId)}</ns:sessionId>
         <ns:eventDto>
            <ns:TaskId>{EscapeXml(TaskId)}</ns:TaskId>
            <ns:EventType>{EscapeXml(EventType)}</ns:EventType>
            <ns:Description>{EscapeXml(Description)}</ns:Description>
            <ns:EventDateUtc>{EscapeXml(EventDateUtc)}</ns:EventDateUtc>
         </ns:eventDto>
      </ns:CreateEvent>
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

    /// <summary>
    /// Formats a DateTime to the CAFM expected format.
    /// </summary>
    public static string FormatDateForCAFM(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return string.Empty;

        return dateTime.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
    }
}
