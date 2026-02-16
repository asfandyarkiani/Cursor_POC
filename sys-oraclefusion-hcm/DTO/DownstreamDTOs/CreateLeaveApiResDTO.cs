namespace sys_oraclefusion_hcm.DTO.DownstreamDTOs
{
    public class CreateLeaveApiResDTO
    {
        public long PersonAbsenceEntryId { get; set; }
        public string? AbsenceType { get; set; }
        public string? Employer { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? AbsenceStatusCd { get; set; }
        public string? ApprovalStatusCd { get; set; }
        public int? StartDateDuration { get; set; }
        public int? EndDateDuration { get; set; }
        public string? PersonNumber { get; set; }
        public int? Duration { get; set; }
        public string? CreationDate { get; set; }
        public string? LastUpdateDate { get; set; }
    }
}
