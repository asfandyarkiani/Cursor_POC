using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystemLayer.DTO.AtomicHandlerDTOs
{
    public class AuthenticationRequestDTO : IDownStreamRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required");
                
            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    error: ("CAFM_VAL_0001", "Validation failed"),
                    errorDetails: errors,
                    stepName: "AuthenticationRequestDTO / ValidateDownStreamRequestParameters"
                );
        }
    }
}
