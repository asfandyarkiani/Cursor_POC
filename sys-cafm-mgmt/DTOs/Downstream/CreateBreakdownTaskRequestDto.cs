using Core.SystemLayer.DTOs;

namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Downstream request DTO for FSI CreateBreakdownTask SOAP call
    /// Based on Boomi profile: CreateBreakdownTask EQ+_to_CAFM_Create
    /// </summary>
    public class CreateBreakdownTaskRequestDto : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        
        // Reporter details
        public string? ReporterName { get; set; }
        public string? ReporterEmail { get; set; }
        public string? ReporterPhone { get; set; }
        
        // Location details
        public string? BuildingId { get; set; }
        public string? LocationId { get; set; }
        
        // Category details
        public string? CategoryId { get; set; }
        public string? DisciplineId { get; set; }
        public string? InstructionId { get; set; }
        public string? PriorityId { get; set; }
        
        // Task details
        public string? CallId { get; set; }
        public string? LongDescription { get; set; }
        public string? ScheduledDateUtc { get; set; }
        public string? RaisedDateUtc { get; set; }
        
        // Contract details
        public string? ContractId { get; set; }
        public string? CallerSourceId { get; set; }

        public void ValidateDownStreamRequestParameters()
        {
            if (string.IsNullOrWhiteSpace(SessionId))
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Session ID is required",
                    "DOWNSTREAM_VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "CreateBreakdownTaskRequestDto.ValidateDownStreamRequestParameters"
                );
            }

            if (string.IsNullOrWhiteSpace(CallId))
            {
                throw new Core.Exceptions.RequestValidationFailureException(
                    "Call ID (Service Request Number) is required",
                    "DOWNSTREAM_VALIDATION_ERROR",
                    System.Net.HttpStatusCode.BadRequest,
                    stepName: "CreateBreakdownTaskRequestDto.ValidateDownStreamRequestParameters"
                );
            }
        }

        /// <summary>
        /// Build SOAP envelope for FSI CreateBreakdownTask request
        /// </summary>
        public string ToSoapEnvelope()
        {
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns=""http://www.fsi.co.uk/services/evolution/04/09"" xmlns:ns1=""http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.Entities.ServiceModel"" xmlns:ns2=""http://schemas.datacontract.org/2004/07/Fsi.Platform.Contracts.ServiceModel"">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:CreateBreakdownTask>
         <ns:sessionId>{SessionId}</ns:sessionId>
         <ns:breakdownTaskDto>
            <ns2:BuildingId>{BuildingId ?? string.Empty}</ns2:BuildingId>
            <ns2:CategoryId>{CategoryId ?? string.Empty}</ns2:CategoryId>
            <ns2:ContractId>{ContractId ?? string.Empty}</ns2:ContractId>
            <ns2:DisciplineId>{DisciplineId ?? string.Empty}</ns2:DisciplineId>
            <ns2:InstructionId>{InstructionId ?? string.Empty}</ns2:InstructionId>
            <ns2:LocationId>{LocationId ?? string.Empty}</ns2:LocationId>
            <ns2:LongDescription>{System.Security.SecurityElement.Escape(LongDescription ?? string.Empty)}</ns2:LongDescription>
            <ns2:PriorityId>{PriorityId ?? string.Empty}</ns2:PriorityId>
            <ns2:RaisedDateUtc>{RaisedDateUtc ?? string.Empty}</ns2:RaisedDateUtc>
            <ns2:ScheduledDateUtc>{ScheduledDateUtc ?? string.Empty}</ns2:ScheduledDateUtc>
            <ns2:ReporterName>{System.Security.SecurityElement.Escape(ReporterName ?? string.Empty)}</ns2:ReporterName>
            <ns2:Phone>{ReporterPhone ?? string.Empty}</ns2:Phone>
            <ns2:BDET_EMAIL>{ReporterEmail ?? string.Empty}</ns2:BDET_EMAIL>
            <ns2:CallId>{CallId ?? string.Empty}</ns2:CallId>
            <ns2:BDET_CALLER_SOURCE_ID>{CallerSourceId ?? string.Empty}</ns2:BDET_CALLER_SOURCE_ID>
         </ns:breakdownTaskDto>
      </ns:CreateBreakdownTask>
   </soapenv:Body>
</soapenv:Envelope>";
        }
    }
}
