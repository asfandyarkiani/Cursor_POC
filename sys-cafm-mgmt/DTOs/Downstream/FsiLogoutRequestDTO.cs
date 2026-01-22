using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream;

/// <summary>
/// Request DTO for FSI Logout SOAP call
/// </summary>
public class FsiLogoutRequestDTO : IDownStreamRequestDTO
{
    public string? SessionId { get; set; }

    public void ValidateDownStreamRequestParameters()
    {
        if (string.IsNullOrWhiteSpace(SessionId))
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                "SessionId is required for FSI Logout",
                "DS_VALIDATION_ERROR",
                stepName: nameof(FsiLogoutRequestDTO));
        }
    }

    public string ToSoapRequest()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:LogOut>
         <ns:sessionId>{SessionId}</ns:sessionId>
      </ns:LogOut>
   </soapenv:Body>
</soapenv:Envelope>";
    }
}
