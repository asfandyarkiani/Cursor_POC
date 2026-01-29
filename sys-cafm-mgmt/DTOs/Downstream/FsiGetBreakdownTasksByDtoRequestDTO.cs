using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream;

/// <summary>
/// Request DTO for FSI GetBreakdownTasksByDto SOAP call
/// </summary>
public class FsiGetBreakdownTasksByDtoRequestDTO : IDownStreamRequestDTO
{
    public string? SessionId { get; set; }
    public string? TaskNumber { get; set; }

    public void ValidateDownStreamRequestParameters()
    {
        if (string.IsNullOrWhiteSpace(SessionId))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "SessionId is required for GetBreakdownTasksByDto",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiGetBreakdownTasksByDtoRequestDTO));
        }

        if (string.IsNullOrWhiteSpace(TaskNumber))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "TaskNumber is required for GetBreakdownTasksByDto",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiGetBreakdownTasksByDtoRequestDTO));
        }
    }

    public string ToSoapRequest()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Tasks.Contracts.Entities"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:GetBreakdownTasksByDto>
            <ns:sessionId>{SessionId}</ns:sessionId>
            <ns:dto>
                <fsi:BDET_NO>{TaskNumber}</fsi:BDET_NO>
            </ns:dto>
        </ns:GetBreakdownTasksByDto>
    </soapenv:Body>
</soapenv:Envelope>";
    }
}

/// <summary>
/// Response DTO for FSI GetBreakdownTasksByDto SOAP call
/// </summary>
public class FsiGetBreakdownTasksByDtoResponseDTO
{
    public string? TaskId { get; set; }
    public string? TaskNumber { get; set; }
    public string? Status { get; set; }
    public string? BuildingId { get; set; }
    public string? LocationId { get; set; }
}
