using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.HandlerDTOs.GetInstructionSetsByDtoDTO
{
    public class GetInstructionSetsByDtoReqDTO : IRequestSysDTO
    {
        public string SubCategory { get; set; } = string.Empty;

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(SubCategory))
                errors.Add("SubCategory is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException() { ErrorProperties = errors };
        }
    }
}
