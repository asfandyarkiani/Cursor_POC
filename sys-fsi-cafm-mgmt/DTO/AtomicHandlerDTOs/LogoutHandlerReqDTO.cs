using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace FsiCafmSystem.DTO.AtomicHandlerDTOs
{
    public class LogoutHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
            {
                errors.Add("SessionId is required.");
            }
            
            if (errors.Count > 0)
            {
                throw new RequestValidationFailureException(
                    error: ("FSI_VAL_0002", "Validation failed"),
                    errorDetails: errors,
                    stepName: "LogoutHandlerReqDTO / ValidateDownStreamRequestParameters");
            }
        }
    }
}
