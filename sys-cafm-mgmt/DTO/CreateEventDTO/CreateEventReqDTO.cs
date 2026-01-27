using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace sys_cafm_mgmt.DTO.CreateEventDTO
{
    public class CreateEventReqDTO : IRequestSysDTO
    {
        public int BreakdownTaskId { get; set; }
        public string EventDescription { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (BreakdownTaskId <= 0)
                errors.Add("BreakdownTaskId must be greater than 0");
            
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateEventReqDTO.cs / Executing ValidateAPIRequestParameters");
        }
    }
}
