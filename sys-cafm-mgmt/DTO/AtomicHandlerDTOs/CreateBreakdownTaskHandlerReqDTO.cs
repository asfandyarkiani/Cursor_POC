using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace sys_cafm_mgmt.DTO.AtomicHandlerDTOs
{
    public class CreateBreakdownTaskHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string ReporterPhoneNumber { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int DisciplineId { get; set; }
        public int PriorityId { get; set; }
        public int BuildingId { get; set; }
        public int LocationId { get; set; }
        public int InstructionId { get; set; }
        public string ScheduledDateUtc { get; set; } = string.Empty;
        public string RaisedDateUtc { get; set; } = string.Empty;
        public int ContractId { get; set; }
        public string CallerSourceId { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");
            
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required");
            
            if (string.IsNullOrWhiteSpace(Description))
                errors.Add("Description is required");
            
            if (CategoryId <= 0)
                errors.Add("CategoryId must be greater than 0");
            
            if (BuildingId <= 0)
                errors.Add("BuildingId must be greater than 0");
            
            if (LocationId <= 0)
                errors.Add("LocationId must be greater than 0");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateBreakdownTaskHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters");
        }
    }
}
