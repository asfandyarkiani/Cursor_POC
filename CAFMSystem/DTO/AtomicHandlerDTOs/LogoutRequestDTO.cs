using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    /// <summary>
    /// Request DTO for LogoutAtomicHandler (internal use only).
    /// Used by CustomAuthenticationMiddleware to logout from CAFM.
    /// </summary>
    public class LogoutRequestDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required.");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "LogoutRequestDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
