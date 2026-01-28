using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.GetBreakdownTasksByDtoDTO
{
    /// <summary>
    /// Request DTO for GetBreakdownTasksByDto API.
    /// Checks if breakdown task already exists in CAFM based on CallId (serviceRequestNumber).
    /// </summary>
    public class GetBreakdownTasksByDtoReqDTO : IRequestSysDTO
    {
        public string ServiceRequestNumber { get; set; } = string.Empty;

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required.");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "GetBreakdownTasksByDtoReqDTO.cs / Executing ValidateAPIRequestParameters"
                );
        }
    }
}
