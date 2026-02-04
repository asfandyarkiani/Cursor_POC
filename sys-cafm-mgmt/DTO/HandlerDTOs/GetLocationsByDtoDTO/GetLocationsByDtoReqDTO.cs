using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.HandlerDTOs.GetLocationsByDtoDTO
{
    public class GetLocationsByDtoReqDTO : IRequestSysDTO
    {
        public string UnitCode { get; set; } = string.Empty;

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(UnitCode))
                errors.Add("UnitCode is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException() { ErrorProperties = errors };
        }
    }
}
