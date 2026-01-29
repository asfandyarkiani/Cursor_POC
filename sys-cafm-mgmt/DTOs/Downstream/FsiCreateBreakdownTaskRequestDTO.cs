using Core.SystemLayer.DTOs;
using System.Xml;

namespace SysCafmMgmt.DTOs.Downstream;

/// <summary>
/// Request DTO for FSI CreateBreakdownTask SOAP call
/// </summary>
public class FsiCreateBreakdownTaskRequestDTO : IDownStreamRequestDTO
{
    public string? SessionId { get; set; }
    public string? ReporterName { get; set; }
    public string? ReporterEmail { get; set; }
    public string? Phone { get; set; }
    public string? CallId { get; set; }
    public string? CategoryId { get; set; }
    public string? DisciplineId { get; set; }
    public string? PriorityId { get; set; }
    public string? BuildingId { get; set; }
    public string? LocationId { get; set; }
    public string? InstructionId { get; set; }
    public string? LongDescription { get; set; }
    public string? ScheduledDateUtc { get; set; }
    public string? RaisedDateUtc { get; set; }
    public string? ContractId { get; set; }
    public string? CallerSourceId { get; set; }

    public void ValidateDownStreamRequestParameters()
    {
        if (string.IsNullOrWhiteSpace(SessionId))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "SessionId is required for CreateBreakdownTask",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiCreateBreakdownTaskRequestDTO));
        }

        if (string.IsNullOrWhiteSpace(CategoryId))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "CategoryId is required for CreateBreakdownTask",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiCreateBreakdownTaskRequestDTO));
        }

        if (string.IsNullOrWhiteSpace(BuildingId))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "BuildingId is required for CreateBreakdownTask",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiCreateBreakdownTaskRequestDTO));
        }
    }

    public string ToSoapRequest()
    {
        // Escape XML special characters in description
        var escapedDescription = XmlEscape(LongDescription ?? string.Empty);
        var escapedReporterName = XmlEscape(ReporterName ?? string.Empty);

        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi=""http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel""
                  xmlns:fsi1=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Tasks.Contracts.Entities"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:CreateBreakdownTask>
            <ns:sessionId>{SessionId}</ns:sessionId>
            <ns:breakdownTaskDto>
                <fsi1:BuildingId>{BuildingId}</fsi1:BuildingId>
                <fsi1:CategoryId>{CategoryId}</fsi1:CategoryId>
                <fsi1:CallId>{CallId}</fsi1:CallId>
                <fsi1:ContractId>{ContractId}</fsi1:ContractId>
                <fsi1:DisciplineId>{DisciplineId}</fsi1:DisciplineId>
                <fsi1:InstructionId>{InstructionId}</fsi1:InstructionId>
                <fsi1:LocationId>{LocationId}</fsi1:LocationId>
                <fsi1:LongDescription>{escapedDescription}</fsi1:LongDescription>
                <fsi1:Phone>{Phone}</fsi1:Phone>
                <fsi1:PriorityId>{PriorityId}</fsi1:PriorityId>
                <fsi1:RaisedDateUtc>{RaisedDateUtc}</fsi1:RaisedDateUtc>
                <fsi1:ReporterName>{escapedReporterName}</fsi1:ReporterName>
                <fsi1:ScheduledDateUtc>{ScheduledDateUtc}</fsi1:ScheduledDateUtc>
                <fsi1:BDET_EMAIL>{ReporterEmail}</fsi1:BDET_EMAIL>
                <fsi1:BDET_CALLER_SOURCE_ID>{CallerSourceId}</fsi1:BDET_CALLER_SOURCE_ID>
            </ns:breakdownTaskDto>
        </ns:CreateBreakdownTask>
    </soapenv:Body>
</soapenv:Envelope>";
    }

    private static string XmlEscape(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}

/// <summary>
/// Response DTO for FSI CreateBreakdownTask SOAP call
/// </summary>
public class FsiCreateBreakdownTaskResponseDTO
{
    public string? TaskId { get; set; }
    public string? TaskNumber { get; set; }
    public string? OperationResult { get; set; }
    public string? Message { get; set; }
}
