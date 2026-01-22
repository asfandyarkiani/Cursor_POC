using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream;

/// <summary>
/// Request DTO for FSI GetInstructionSetsByDto SOAP call
/// </summary>
public class FsiGetInstructionSetsByDtoRequestDTO : IDownStreamRequestDTO
{
    public string? SessionId { get; set; }
    public string? InDescription { get; set; }

    public void ValidateDownStreamRequestParameters()
    {
        if (string.IsNullOrWhiteSpace(SessionId))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "SessionId is required for GetInstructionSetsByDto",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiGetInstructionSetsByDtoRequestDTO));
        }

        if (string.IsNullOrWhiteSpace(InDescription))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "InDescription (SubCategory) is required for GetInstructionSetsByDto",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiGetInstructionSetsByDtoRequestDTO));
        }
    }

    public string ToSoapRequest()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi=""http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel""
                  xmlns:fsi1=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel""
                  xmlns:fsi2=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Tasks.Contracts.Entities"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:GetInstructionSetsByDto>
            <ns:sessionId>{SessionId}</ns:sessionId>
            <ns:dto>
                <fsi2:IN_DESCRIPTION>{InDescription}</fsi2:IN_DESCRIPTION>
            </ns:dto>
        </ns:GetInstructionSetsByDto>
    </soapenv:Body>
</soapenv:Envelope>";
    }
}

/// <summary>
/// Response DTO for FSI GetInstructionSetsByDto SOAP call
/// </summary>
public class FsiGetInstructionSetsByDtoResponseDTO
{
    public string? CategoryId { get; set; }
    public string? DisciplineId { get; set; }
    public string? PriorityId { get; set; }
    public string? InstructionId { get; set; }
    public string? InstructionDescription { get; set; }
}
