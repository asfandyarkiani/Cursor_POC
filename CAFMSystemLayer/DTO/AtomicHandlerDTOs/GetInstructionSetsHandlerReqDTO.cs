using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystemLayer.DTO.AtomicHandlerDTOs
{
    public class GetInstructionSetsHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");
                
            if (string.IsNullOrWhiteSpace(CategoryName))
                errors.Add("CategoryName is required");
                
            if (string.IsNullOrWhiteSpace(SubCategory))
                errors.Add("SubCategory is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    error: ("CAFM_VAL_0004", "Validation failed"),
                    errorDetails: errors,
                    stepName: "GetInstructionSetsHandlerReqDTO / ValidateDownStreamRequestParameters"
                );
        }
    }
}
