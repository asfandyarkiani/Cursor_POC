using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    /// <summary>
    /// Request DTO for CreateEventAtomicHandler.
    /// Internal DTO used by Handler to call Atomic Handler.
    /// </summary>
    public class CreateEventHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string EventType { get; set; } = "Recurring";

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required.");

            if (string.IsNullOrWhiteSpace(TaskId))
                errors.Add("TaskId is required.");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateEventHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
