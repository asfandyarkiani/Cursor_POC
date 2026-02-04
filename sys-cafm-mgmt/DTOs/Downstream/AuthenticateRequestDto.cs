using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Downstream request DTO for FSI Authenticate SOAP call
    /// </summary>
    public class AuthenticateRequestDto : IDownStreamRequestDTO
    {
        public string LoginName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            if (string.IsNullOrWhiteSpace(LoginName))
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Login name is required for authentication",
                    "DOWNSTREAM_VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "AuthenticateRequestDto.ValidateDownStreamRequestParameters"
                );
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Password is required for authentication",
                    "DOWNSTREAM_VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "AuthenticateRequestDto.ValidateDownStreamRequestParameters"
                );
            }
        }

        /// <summary>
        /// Build SOAP envelope for FSI Authenticate request
        /// </summary>
        public string ToSoapEnvelope()
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
}
