using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Downstream request DTO for FSI LogOut SOAP call
    /// </summary>
    public class LogoutRequestDto : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            if (string.IsNullOrWhiteSpace(SessionId))
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Session ID is required for logout",
                    "DOWNSTREAM_VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "LogoutRequestDto.ValidateDownStreamRequestParameters"
                );
            }
        }

        /// <summary>
        /// Build SOAP envelope for FSI LogOut request
        /// </summary>
        public string ToSoapEnvelope()
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
}
