using OracleFusionHCMSystemLayer.DTO.DownstreamDTOs;

namespace OracleFusionHCMSystemLayer.DTO.CreateAbsenceDTO
{
    public class CreateAbsenceResDTO
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public long? PersonAbsenceEntryId { get; set; }
        public string Success { get; set; } = string.Empty;

        public static CreateAbsenceResDTO Map(CreateAbsenceApiResDTO apiResponse)
        {
            return new CreateAbsenceResDTO
            {
                Status = "success",
                Message = "Data successfully sent to Oracle Fusion",
                PersonAbsenceEntryId = apiResponse.PersonAbsenceEntryId,
                Success = "true"
            };
        }

        public static CreateAbsenceResDTO MapError(string errorMessage)
        {
            return new CreateAbsenceResDTO
            {
                Status = "failure",
                Message = errorMessage,
                PersonAbsenceEntryId = null,
                Success = "false"
            };
        }
    }
}
