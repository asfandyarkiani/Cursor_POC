using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace FsiCafmSystem.DTO.AtomicHandlerDTOs
{
    public class GetInstructionSetsByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
            {
                errors.Add("SessionId is required.");
            }
            
            if (string.IsNullOrWhiteSpace(Description))
            {
                errors.Add("Description is required.");
            }
            
            if (errors.Count > 0)
            {
                throw new RequestValidationFailureException(
                    error: ("FSI_VAL_0004", "Validation failed"),
                    errorDetails: errors,
                    stepName: "GetInstructionSetsByDtoHandlerReqDTO / ValidateDownStreamRequestParameters");
            }
        }
    }
}
