using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMManagementSystem.DTO.AtomicHandlerDTOs
{
    public class GetInstructionSetsByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required.");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "GetInstructionSetsByDtoHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters"
                );
        }
    }
}
