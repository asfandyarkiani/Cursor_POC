using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.HandlerDTOs.GetBreakdownTasksByDtoDTO
{
    public class GetBreakdownTasksByDtoReqDTO : IRequestSysDTO
    {
        public string ServiceRequestNumber { get; set; } = string.Empty;

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException() { ErrorProperties = errors };
        }
    }
}
