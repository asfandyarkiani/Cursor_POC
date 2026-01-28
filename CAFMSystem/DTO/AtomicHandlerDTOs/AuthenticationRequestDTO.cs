using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    /// <summary>
    /// Request DTO for AuthenticateAtomicHandler (internal use only).
    /// Used by CustomAuthenticationMiddleware to authenticate with CAFM.
    /// </summary>
    public class AuthenticationRequestDTO : IDownStreamRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required.");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password is required.");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "AuthenticationRequestDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
