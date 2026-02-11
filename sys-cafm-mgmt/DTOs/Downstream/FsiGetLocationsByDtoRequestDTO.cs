using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream;

/// <summary>
/// Request DTO for FSI GetLocationsByDto SOAP call
/// </summary>
public class FsiGetLocationsByDtoRequestDTO : IDownStreamRequestDTO
{
    public string? SessionId { get; set; }
    public string? BarCode { get; set; }

    public void ValidateDownStreamRequestParameters()
    {
        if (string.IsNullOrWhiteSpace(SessionId))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "SessionId is required for GetLocationsByDto",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiGetLocationsByDtoRequestDTO));
        }

        if (string.IsNullOrWhiteSpace(BarCode))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "BarCode (UnitCode) is required for GetLocationsByDto",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiGetLocationsByDtoRequestDTO));
        }
    }

    public string ToSoapRequest()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09""
                  xmlns:fsi=""http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel""
                  xmlns:fsi1=""http://schemas.datacontract.org/2004/07/Fsi.Concept.Contracts.Entities.ServiceModel"">
    <soapenv:Header/>
    <soapenv:Body>
        <ns:GetLocationsByDto>
            <ns:sessionId>{SessionId}</ns:sessionId>
            <ns:locationDto>
                <fsi1:BarCode>{BarCode}</fsi1:BarCode>
            </ns:locationDto>
        </ns:GetLocationsByDto>
    </soapenv:Body>
</soapenv:Envelope>";
    }
}

/// <summary>
/// Response DTO for FSI GetLocationsByDto SOAP call
/// </summary>
public class FsiGetLocationsByDtoResponseDTO
{
    public string? BuildingId { get; set; }
    public string? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? BarCode { get; set; }
}
