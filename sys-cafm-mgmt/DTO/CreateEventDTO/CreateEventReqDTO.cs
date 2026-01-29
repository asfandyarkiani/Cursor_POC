using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.CreateEventDTO
{
    public class CreateEventReqDTO : IRequestSysDTO
    {
        public string BreakdownTaskId { get; set; } = string.Empty;

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(BreakdownTaskId))
                errors.Add("BreakdownTaskId is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateEventReqDTO.cs / Executing ValidateAPIRequestParameters"
                );
        }
    }
}
