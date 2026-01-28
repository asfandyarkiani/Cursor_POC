using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class GetLocationsByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string UnitCode { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(UnitCode))
                errors.Add("UnitCode is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "GetLocationsByDtoHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
