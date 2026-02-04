using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.HandlerDTOs.CreateEventDTO
{
    public class CreateEventReqDTO : IRequestSysDTO
    {
        public string TaskId { get; set; } = string.Empty;
        public string? Comments { get; set; }

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(TaskId))
                errors.Add("TaskId is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException() { ErrorProperties = errors };
        }
    }
}
