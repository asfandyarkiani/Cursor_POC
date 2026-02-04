using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream request DTO for CAFM GetLocationsByDto SOAP call.
/// </summary>
public class CAFMGetLocationsDownstreamRequestDto : IDownStreamRequestDTO
{
    /// <summary>
    /// Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Property/Building name to search for.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Unit code within the property.
    /// </summary>
    public string UnitCode { get; set; } = string.Empty;

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

        if (string.IsNullOrWhiteSpace(PropertyName))
            errors.Add("PropertyName is required.");

        if (string.IsNullOrWhiteSpace(Url))
            errors.Add("Url is required.");

        if (string.IsNullOrWhiteSpace(SoapAction))
            errors.Add("SoapAction is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CAFMGetLocationsDownstreamRequestDto));
        }
    }

    /// <summary>
    /// Generates the SOAP XML envelope for the GetLocationsByDto request.
    /// </summary>
    public string ToSoapXml()
    {
        var unitCodeElement = string.IsNullOrWhiteSpace(UnitCode) 
            ? string.Empty 
            : $"<ns:LOC_UNIT_ID>{EscapeXml(UnitCode)}</ns:LOC_UNIT_ID>";

        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:GetLocationsByDto>
         <ns:sessionId>{EscapeXml(SessionId)}</ns:sessionId>
         <ns:locationDto>
            <ns:LOC_BUILDING_NAME>{EscapeXml(PropertyName)}</ns:LOC_BUILDING_NAME>
            {unitCodeElement}
         </ns:locationDto>
      </ns:GetLocationsByDto>
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
