namespace OracleFusionHcmSystemLayer.DTO.DownstreamDTOs
{
    public class CreateAbsenceApiResDTO
    {
        public long? PersonAbsenceEntryId { get; set; }
        public string? AbsenceType { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? AbsenceStatusCd { get; set; }
        public string? ApprovalStatusCd { get; set; }
    }
}
