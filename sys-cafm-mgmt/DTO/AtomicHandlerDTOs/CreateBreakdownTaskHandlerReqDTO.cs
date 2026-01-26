using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class CreateBreakdownTaskHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string CallerSourceId { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string BuildingId { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string DisciplineId { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string PriorityId { get; set; } = string.Empty;
        public string LoggedBy { get; set; } = "EQARCOM+";
        public string RaisedDate { get; set; } = string.Empty;
        public string ScheduledDate { get; set; } = string.Empty;
        public string ScheduledEndTime { get; set; } = string.Empty;
        public string ScheduledStartTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SubStatus { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        public string InstructionId { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");

            if (string.IsNullOrWhiteSpace(CallerSourceId))
                errors.Add("CallerSourceId is required");

            if (string.IsNullOrWhiteSpace(BuildingId))
                errors.Add("BuildingId is required");

            if (string.IsNullOrWhiteSpace(CategoryId))
                errors.Add("CategoryId is required");

            if (string.IsNullOrWhiteSpace(LocationId))
                errors.Add("LocationId is required");

            if (string.IsNullOrWhiteSpace(RaisedDate))
                errors.Add("RaisedDate is required");

            if (errors.Count > 0)
                throw new RequestValidationFailureException()
                {
                    ErrorProperties = errors
                };
        }
    }
}
