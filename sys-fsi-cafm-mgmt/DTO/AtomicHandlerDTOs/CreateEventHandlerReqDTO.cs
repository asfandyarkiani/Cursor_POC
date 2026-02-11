using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace FsiCafmSystem.DTO.AtomicHandlerDTOs
{
    public class CreateEventHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
            {
                errors.Add("SessionId is required.");
            }
            
            if (string.IsNullOrWhiteSpace(TaskId))
            {
                errors.Add("TaskId is required.");
            }
            
            if (errors.Count > 0)
            {
                throw new RequestValidationFailureException(
                    error: ("FSI_VAL_0007", "Validation failed"),
                    errorDetails: errors,
                    stepName: "CreateEventHandlerReqDTO / ValidateDownStreamRequestParameters");
            }
        }
    }
}
