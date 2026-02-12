using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream request DTO for CAFM GetInstructionSetsByDto SOAP call.
/// </summary>
public class CAFMGetInstructionSetsDownstreamRequestDto : IDownStreamRequestDTO
{
    /// <summary>
    /// Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Category name to search for instruction sets.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Sub-category name within the category.
    /// </summary>
    public string SubCategory { get; set; } = string.Empty;

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

        if (string.IsNullOrWhiteSpace(CategoryName))
            errors.Add("CategoryName is required.");

        if (string.IsNullOrWhiteSpace(Url))
            errors.Add("Url is required.");

        if (string.IsNullOrWhiteSpace(SoapAction))
            errors.Add("SoapAction is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CAFMGetInstructionSetsDownstreamRequestDto));
        }
    }

    /// <summary>
    /// Generates the SOAP XML envelope for the GetInstructionSetsByDto request.
    /// </summary>
    public string ToSoapXml()
    {
        var subCategoryElement = string.IsNullOrWhiteSpace(SubCategory)
            ? string.Empty
            : $"<ns:IN_SUB_CATEGORY>{EscapeXml(SubCategory)}</ns:IN_SUB_CATEGORY>";

        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:GetInstructionSetsByDto>
         <ns:sessionId>{EscapeXml(SessionId)}</ns:sessionId>
         <ns:instructionSetDto>
            <ns:IN_CATEGORY>{EscapeXml(CategoryName)}</ns:IN_CATEGORY>
            {subCategoryElement}
         </ns:instructionSetDto>
      </ns:GetInstructionSetsByDto>
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
