using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class GetBreakdownTasksByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");
            
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    error: ("SYS_VAL_0005", "Validation failed"),
                    errorDetails: errors,
                    stepName: "GetBreakdownTasksByDtoHandlerReqDTO / ValidateDownStreamRequestParameters");
        }
    }
}
