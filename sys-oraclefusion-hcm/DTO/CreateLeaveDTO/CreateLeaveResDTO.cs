using OracleFusionHcm.DTO.DownstreamDTOs;

namespace OracleFusionHcm.DTO.CreateLeaveDTO
{
    public class CreateLeaveResDTO
    {
        public long PersonAbsenceEntryId { get; set; }
        public string Status { get; set; } = string.Empty;

        public static CreateLeaveResDTO Map(CreateLeaveApiResDTO apiResponse)
        {
            return new CreateLeaveResDTO
            {
                PersonAbsenceEntryId = apiResponse.PersonAbsenceEntryId ?? 0,
                Status = apiResponse.AbsenceStatusCd ?? "Unknown"
            };
        }
    }
}
