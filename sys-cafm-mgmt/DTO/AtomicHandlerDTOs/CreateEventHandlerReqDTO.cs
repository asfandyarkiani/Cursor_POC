using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace sys_cafm_mgmt.DTO.AtomicHandlerDTOs
{
    public class CreateEventHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public int BreakdownTaskId { get; set; }
        public string EventDescription { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");
            
            if (BreakdownTaskId <= 0)
                errors.Add("BreakdownTaskId must be greater than 0");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateEventHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters");
        }
    }
}
