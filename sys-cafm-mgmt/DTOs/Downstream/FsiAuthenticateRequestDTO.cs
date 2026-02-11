using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream;

/// <summary>
/// Request DTO for FSI Authenticate SOAP call
/// </summary>
public class FsiAuthenticateRequestDTO : IDownStreamRequestDTO
{
    public string? LoginName { get; set; }
    public string? Password { get; set; }

    public void ValidateDownStreamRequestParameters()
    {
        if (string.IsNullOrWhiteSpace(LoginName))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "LoginName is required for FSI Authentication",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiAuthenticateRequestDTO));
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "Password is required for FSI Authentication",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiAuthenticateRequestDTO));
        }
    }

    public string ToSoapRequest()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:Authenticate>
         <ns:loginName>{LoginName}</ns:loginName>
         <ns:password>{Password}</ns:password>
      </ns:Authenticate>
   </soapenv:Body>
</soapenv:Envelope>";
    }
}

/// <summary>
/// Response DTO for FSI Authenticate SOAP call
/// </summary>
public class FsiAuthenticateResponseDTO
{
    public string? SessionId { get; set; }
    public string? OperationResult { get; set; }
    public string? EvolutionVersion { get; set; }
}
