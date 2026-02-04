using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.HandlerDTOs.CreateBreakdownTaskDTO
{
    public class CreateBreakdownTaskReqDTO : IRequestSysDTO
    {
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string? ReporterName { get; set; }
        public string? ReporterEmail { get; set; }
        public string? ReporterPhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? BuildingId { get; set; }
        public string? LocationId { get; set; }
        public string? CategoryId { get; set; }
        public string? DisciplineId { get; set; }
        public string? PriorityId { get; set; }
        public string? InstructionId { get; set; }
        public string? ContractId { get; set; }
        public string? CallerSourceId { get; set; }
        public string? ScheduledDateUtc { get; set; }
        public string? RaisedDateUtc { get; set; }

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException() { ErrorProperties = errors };
        }
    }
}
