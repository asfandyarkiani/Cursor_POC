using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace sys_cafm_mgmt.DTO.AtomicHandlerDTOs
{
    public class LogoutHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "LogoutHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters");
        }
    }
}
