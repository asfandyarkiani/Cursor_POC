using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Downstream request DTO for FSI GetInstructionSetsByDto SOAP call
    /// </summary>
    public class GetInstructionSetsByDtoRequestDto : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? SubCategory { get; set; }

        public void ValidateDownStreamRequestParameters()
        {
            if (string.IsNullOrWhiteSpace(SessionId))
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Session ID is required",
                    "DOWNSTREAM_VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "GetInstructionSetsByDtoRequestDto.ValidateDownStreamRequestParameters"
                );
            }
        }

        /// <summary>
        /// Build SOAP envelope for FSI GetInstructionSetsByDto request
        /// </summary>
        public string ToSoapEnvelope()
        {
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"" xmlns:ns1=""http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:GetInstructionSetsByDto>
         <ns:sessionId>{SessionId}</ns:sessionId>
         <ns:instructionSetFilterDto>
            <ns1:Code>{CategoryName ?? string.Empty}</ns1:Code>
         </ns:instructionSetFilterDto>
      </ns:GetInstructionSetsByDto>
   </soapenv:Body>
</soapenv:Envelope>";
        }
    }
}
