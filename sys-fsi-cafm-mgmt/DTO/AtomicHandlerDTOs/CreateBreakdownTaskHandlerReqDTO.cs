using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace FsiCafmSystem.DTO.AtomicHandlerDTOs
{
    public class CreateBreakdownTaskHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string BuildingId { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string DisciplineId { get; set; } = string.Empty;
        public string PriorityId { get; set; } = string.Empty;
        public string InstructionId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string ReporterPhoneNumber { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string Technician { get; set; } = string.Empty;
        public string SourceOrgId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SubStatus { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ScheduledDate { get; set; } = string.Empty;
        public string ScheduledTimeStart { get; set; } = string.Empty;
        public string ScheduledTimeEnd { get; set; } = string.Empty;
        public string ScheduledDateUtc { get; set; } = string.Empty;
        public string RaisedDateUtc { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
            {
                errors.Add("SessionId is required.");
            }
            
            if (string.IsNullOrWhiteSpace(Description))
            {
                errors.Add("Description is required.");
            }
            
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
            {
                errors.Add("ServiceRequestNumber is required.");
            }
            
            if (errors.Count > 0)
            {
                throw new RequestValidationFailureException(
                    error: ("FSI_VAL_0006", "Validation failed"),
                    errorDetails: errors,
                    stepName: "CreateBreakdownTaskHandlerReqDTO / ValidateDownStreamRequestParameters");
            }
        }
    }
}
