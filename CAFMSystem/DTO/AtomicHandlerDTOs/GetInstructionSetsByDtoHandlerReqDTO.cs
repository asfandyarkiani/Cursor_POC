using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    /// <summary>
    /// Request DTO for GetInstructionSetsByDtoAtomicHandler.
    /// Internal DTO used by Handler to call Atomic Handler.
    /// </summary>
    public class GetInstructionSetsByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required.");

            if (string.IsNullOrWhiteSpace(CategoryName))
                errors.Add("CategoryName is required.");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "GetInstructionSetsByDtoHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
