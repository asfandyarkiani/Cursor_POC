using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace CAFMSystem.DTO.AtomicHandlerDTOs
{
    public class GetInstructionSetsByDtoHandlerReqDTO : IDownStreamRequestDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public string InstructionDescription { get; set; } = string.Empty;

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SessionId))
                errors.Add("SessionId is required");

            if (string.IsNullOrWhiteSpace(InstructionDescription))
                errors.Add("InstructionDescription is required");

            if (errors.Count > 0)
                throw new RequestValidationFailureException()
                {
                    ErrorProperties = errors
                };
        }
    }
}
