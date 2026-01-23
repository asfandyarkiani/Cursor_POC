using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class GetInstructionSetsByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");
            
            if (string.IsNullOrWhiteSpace(SubCategory))
                errors.Add("SubCategory is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    error: ("SYS_VAL_0004", "Validation failed"),
                    errorDetails: errors,
                    stepName: "GetInstructionSetsByDtoHandlerReqDTO / ValidateDownStreamRequestParameters");
        }
    }
}
