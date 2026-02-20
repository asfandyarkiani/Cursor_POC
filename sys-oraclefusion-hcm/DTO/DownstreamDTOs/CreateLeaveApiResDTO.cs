namespace OracleFusionHcm.DTO.DownstreamDTOs
{
    public class CreateLeaveApiResDTO
    {
        public long? PersonAbsenceEntryId { get; set; }
        public string? AbsenceStatusCd { get; set; }
        public string? AbsenceName { get; set; }
        public string? PersonNumber { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
