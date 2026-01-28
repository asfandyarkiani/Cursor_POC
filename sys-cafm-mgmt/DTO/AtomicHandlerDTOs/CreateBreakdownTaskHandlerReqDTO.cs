using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class CreateBreakdownTaskHandlerReqDTO : IDownStreamRequestDTO
    {
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string ReporterPhoneNumber { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string DisciplineId { get; set; } = string.Empty;
        public string PriorityId { get; set; } = string.Empty;
        public string BuildingId { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string InstructionId { get; set; } = string.Empty;
        public string ScheduledDateUtc { get; set; } = string.Empty;
        public string RaisedDateUtc { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        public string SourceOrgId { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(ReporterName))
                errors.Add("ReporterName is required");
                
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required");
                
            if (string.IsNullOrWhiteSpace(Description))
                errors.Add("Description is required");
                
            if (string.IsNullOrWhiteSpace(RaisedDateUtc))
                errors.Add("RaisedDateUtc is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateBreakdownTaskHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
