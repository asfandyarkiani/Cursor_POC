using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class GetBreakdownTasksByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string ServiceRequestNumber { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "GetBreakdownTasksByDtoHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
