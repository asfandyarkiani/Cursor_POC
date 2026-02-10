using OracleFusionHCMSystem.DTO.DownstreamDTOs;

namespace OracleFusionHCMSystem.DTO.CreateLeaveDTO
{
    public class CreateLeaveResDTO
    {
        public long PersonAbsenceEntryId { get; set; }
        public string PersonNumber { get; set; } = string.Empty;
        public string AbsenceType { get; set; } = string.Empty;
        public string Employer { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string AbsenceStatusCd { get; set; } = string.Empty;
        public string ApprovalStatusCd { get; set; } = string.Empty;
        public int StartDateDuration { get; set; }
        public int EndDateDuration { get; set; }

        public static CreateLeaveResDTO Map(CreateLeaveApiResDTO apiResponse)
        {
            return new CreateLeaveResDTO
            {
                PersonAbsenceEntryId = apiResponse.PersonAbsenceEntryId ?? 0,
                PersonNumber = apiResponse.PersonNumber ?? string.Empty,
                AbsenceType = apiResponse.AbsenceType ?? string.Empty,
                Employer = apiResponse.Employer ?? string.Empty,
                StartDate = apiResponse.StartDate ?? string.Empty,
                EndDate = apiResponse.EndDate ?? string.Empty,
                AbsenceStatusCd = apiResponse.AbsenceStatusCd ?? string.Empty,
                ApprovalStatusCd = apiResponse.ApprovalStatusCd ?? string.Empty,
                StartDateDuration = apiResponse.StartDateDuration ?? 0,
                EndDateDuration = apiResponse.EndDateDuration ?? 0
            };
        }
    }
}
