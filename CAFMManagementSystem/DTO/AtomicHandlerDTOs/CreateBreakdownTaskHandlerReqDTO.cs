using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMManagementSystem.DTO.AtomicHandlerDTOs
{
    public class CreateBreakdownTaskHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string ReporterPhoneNumber { get; set; } = string.Empty;
        public string CallId { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string DisciplineId { get; set; } = string.Empty;
        public string PriorityId { get; set; } = string.Empty;
        public string BuildingId { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string InstructionId { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string ScheduledDateUtc { get; set; } = string.Empty;
        public string RaisedDateUtc { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        public string CallerSourceId { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required.");
            
            if (string.IsNullOrWhiteSpace(CallId))
                errors.Add("CallId is required.");
            
            if (string.IsNullOrWhiteSpace(RaisedDateUtc))
                errors.Add("RaisedDateUtc is required.");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateBreakdownTaskHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
