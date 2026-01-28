using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class GetInstructionSetsByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SubCategory { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SubCategory))
                errors.Add("SubCategory is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "GetInstructionSetsByDtoHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
