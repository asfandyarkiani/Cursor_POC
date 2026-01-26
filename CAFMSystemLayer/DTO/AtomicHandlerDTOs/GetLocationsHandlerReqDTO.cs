using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystemLayer.DTO.AtomicHandlerDTOs
{
    public class GetLocationsHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");
                
            if (string.IsNullOrWhiteSpace(PropertyName))
                errors.Add("PropertyName is required");
                
            if (string.IsNullOrWhiteSpace(UnitCode))
                errors.Add("UnitCode is required");
                
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    error: ("CAFM_VAL_0003", "Validation failed"),
                    errorDetails: errors,
                    stepName: "GetLocationsHandlerReqDTO / ValidateDownStreamRequestParameters"
                );
        }
    }
}
