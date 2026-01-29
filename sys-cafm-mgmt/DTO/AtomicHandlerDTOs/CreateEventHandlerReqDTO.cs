using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class CreateEventHandlerReqDTO : IDownStreamRequestDTO
    {
        public string BreakdownTaskId { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(BreakdownTaskId))
                errors.Add("BreakdownTaskId is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateEventHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
