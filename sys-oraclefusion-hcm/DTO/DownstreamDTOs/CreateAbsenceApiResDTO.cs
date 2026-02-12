namespace OracleFusionHCMSystemLayer.DTO.DownstreamDTOs
{
    /// <summary>
    /// Oracle Fusion HCM API response for absence creation
    /// Matches Oracle Fusion HCM REST API response structure
    /// </summary>
    public class CreateAbsenceApiResDTO
    {
        public long? PersonAbsenceEntryId { get; set; }
        public string? AbsenceStatusCd { get; set; }
        public string? ApprovalStatusCd { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? AbsenceType { get; set; }
        public string? Employer { get; set; }
        public string? PersonNumber { get; set; }
        public int? StartDateDuration { get; set; }
        public int? EndDateDuration { get; set; }
        
        // Additional Oracle Fusion fields (70+ fields available, including these commonly used ones)
        public string? AbsenceName { get; set; }
        public string? AbsenceReason { get; set; }
        public string? Comments { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreationDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? LastUpdateDate { get; set; }
    }
}
