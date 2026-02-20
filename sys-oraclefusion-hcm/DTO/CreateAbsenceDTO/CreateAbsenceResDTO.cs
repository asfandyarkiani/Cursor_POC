using OracleFusionHcmSystemLayer.DTO.DownstreamDTOs;

namespace OracleFusionHcmSystemLayer.DTO.CreateAbsenceDTO
{
    public class CreateAbsenceResDTO
    {
        public long PersonAbsenceEntryId { get; set; }
        public string AbsenceType { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string AbsenceStatusCd { get; set; } = string.Empty;
        public string ApprovalStatusCd { get; set; } = string.Empty;

        public static CreateAbsenceResDTO Map(CreateAbsenceApiResDTO apiResponse)
        {
            return new CreateAbsenceResDTO
            {
                PersonAbsenceEntryId = apiResponse.PersonAbsenceEntryId ?? 0,
                AbsenceType = apiResponse.AbsenceType ?? string.Empty,
                StartDate = apiResponse.StartDate ?? string.Empty,
                EndDate = apiResponse.EndDate ?? string.Empty,
                AbsenceStatusCd = apiResponse.AbsenceStatusCd ?? string.Empty,
                ApprovalStatusCd = apiResponse.ApprovalStatusCd ?? string.Empty
            };
        }
    }
}
