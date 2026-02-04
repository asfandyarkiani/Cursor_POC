using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Downstream request DTO for FSI GetLocationsByDto SOAP call
    /// </summary>
    public class GetLocationsByDtoRequestDto : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string? LocationCode { get; set; }
        public string? BuildingName { get; set; }

        public void ValidateDownStreamRequestParameters()
        {
            if (string.IsNullOrWhiteSpace(SessionId))
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Session ID is required",
                    "DOWNSTREAM_VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "GetLocationsByDtoRequestDto.ValidateDownStreamRequestParameters"
                );
            }
        }

        /// <summary>
        /// Build SOAP envelope for FSI GetLocationsByDto request
        /// </summary>
        public string ToSoapEnvelope()
        {
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"" xmlns:ns1=""http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:GetLocationsByDto>
         <ns:sessionId>{SessionId}</ns:sessionId>
         <ns:locationFilterDto>
            <ns1:Code>{LocationCode ?? string.Empty}</ns1:Code>
         </ns:locationFilterDto>
      </ns:GetLocationsByDto>
   </soapenv:Body>
</soapenv:Envelope>";
        }
    }
}
