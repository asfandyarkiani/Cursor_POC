using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream request DTO for CAFM CreateBreakdownTask SOAP call.
/// </summary>
public class CAFMCreateBreakdownTaskDownstreamRequestDto : IDownStreamRequestDTO
{
    /// <summary>
    /// Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Building ID where the task is located.
    /// </summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>
    /// Location ID within the building.
    /// </summary>
    public string LocationId { get; set; } = string.Empty;

    /// <summary>
    /// Category ID for the task type.
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// Instruction ID for the task.
    /// </summary>
    public string InstructionId { get; set; } = string.Empty;

    /// <summary>
    /// Discipline/Labor ID.
    /// </summary>
    public string DisciplineId { get; set; } = string.Empty;

    /// <summary>
    /// Priority ID for the task.
    /// </summary>
    public string PriorityId { get; set; } = string.Empty;

    /// <summary>
    /// Contract ID for the task.
    /// </summary>
    public string ContractId { get; set; } = string.Empty;

    /// <summary>
    /// Caller source ID (e.g., service request number from EQ+).
    /// </summary>
    public string CallerSourceId { get; set; } = string.Empty;

    /// <summary>
    /// Reporter's name.
    /// </summary>
    public string ReporterName { get; set; } = string.Empty;

    /// <summary>
    /// Reporter's email address.
    /// </summary>
    public string ReporterEmail { get; set; } = string.Empty;

    /// <summary>
    /// Reporter's phone number.
    /// </summary>
    public string ReporterPhone { get; set; } = string.Empty;

    /// <summary>
    /// Description of the task/issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date when the task was raised (UTC) - formatted for CAFM.
    /// </summary>
    public string RaisedDateUtc { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled date for the task (UTC) - formatted for CAFM.
    /// </summary>
    public string ScheduledDateUtc { get; set; } = string.Empty;

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

        if (string.IsNullOrWhiteSpace(BuildingId))
            errors.Add("BuildingId is required.");

        if (string.IsNullOrWhiteSpace(LocationId))
            errors.Add("LocationId is required.");

        if (string.IsNullOrWhiteSpace(CategoryId))
            errors.Add("CategoryId is required.");

        if (string.IsNullOrWhiteSpace(InstructionId))
            errors.Add("InstructionId is required.");

        if (string.IsNullOrWhiteSpace(DisciplineId))
            errors.Add("DisciplineId is required.");

        if (string.IsNullOrWhiteSpace(PriorityId))
            errors.Add("PriorityId is required.");

        if (string.IsNullOrWhiteSpace(ContractId))
            errors.Add("ContractId is required.");

        if (string.IsNullOrWhiteSpace(CallerSourceId))
            errors.Add("CallerSourceId is required.");

        if (string.IsNullOrWhiteSpace(ReporterName))
            errors.Add("ReporterName is required.");

        if (string.IsNullOrWhiteSpace(Url))
            errors.Add("Url is required.");

        if (string.IsNullOrWhiteSpace(SoapAction))
            errors.Add("SoapAction is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CAFMCreateBreakdownTaskDownstreamRequestDto));
        }
    }

    /// <summary>
    /// Generates the SOAP XML envelope for the CreateBreakdownTask request.
    /// </summary>
    public string ToSoapXml()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:CreateBreakdownTask>
         <ns:sessionId>{EscapeXml(SessionId)}</ns:sessionId>
         <ns:breakdownTaskDto>
            <ns:BuildingId>{EscapeXml(BuildingId)}</ns:BuildingId>
            <ns:LocationId>{EscapeXml(LocationId)}</ns:LocationId>
            <ns:CategoryId>{EscapeXml(CategoryId)}</ns:CategoryId>
            <ns:InstructionId>{EscapeXml(InstructionId)}</ns:InstructionId>
            <ns:DisciplineId>{EscapeXml(DisciplineId)}</ns:DisciplineId>
            <ns:PriorityId>{EscapeXml(PriorityId)}</ns:PriorityId>
            <ns:ContractId>{EscapeXml(ContractId)}</ns:ContractId>
            <ns:BDET_CALLER_SOURCE_ID>{EscapeXml(CallerSourceId)}</ns:BDET_CALLER_SOURCE_ID>
            <ns:ReporterName>{EscapeXml(ReporterName)}</ns:ReporterName>
            <ns:BDET_EMAIL>{EscapeXml(ReporterEmail)}</ns:BDET_EMAIL>
            <ns:Phone>{EscapeXml(ReporterPhone)}</ns:Phone>
            <ns:Description>{EscapeXml(Description)}</ns:Description>
            <ns:RaisedDateUtc>{EscapeXml(RaisedDateUtc)}</ns:RaisedDateUtc>
            <ns:ScheduledDateUtc>{EscapeXml(ScheduledDateUtc)}</ns:ScheduledDateUtc>
         </ns:breakdownTaskDto>
      </ns:CreateBreakdownTask>
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
    /// Formats a DateTime to the CAFM expected format with specific millisecond suffix.
    /// Based on Boomi transformation: formattedDate.replace(/(\.\d{3})Z$/, ".0208713Z")
    /// </summary>
    public static string FormatDateForCAFM(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return string.Empty;

        // Format to ISO 8601 with specific millisecond suffix as per Boomi mapping
        return dateTime.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
    }
}
