using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMManagementSystem.DTO.AtomicHandlerDTOs
{
    public class CreateEventHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string BreakdownTaskId { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required.");
            
            if (string.IsNullOrWhiteSpace(BreakdownTaskId))
                errors.Add("BreakdownTaskId is required.");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateEventHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
