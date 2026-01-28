using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.CreateEventDTO
{
    /// <summary>
    /// Request DTO for CreateEvent API.
    /// Links a recurring event to an existing breakdown task in CAFM.
    /// </summary>
    public class CreateEventReqDTO : IRequestSysDTO
    {
        public string TaskId { get; set; } = string.Empty;
        public string EventType { get; set; } = "Recurring";

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(TaskId))
                errors.Add("TaskId is required.");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateEventReqDTO.cs / Executing ValidateAPIRequestParameters"
                );
        }
    }
}
