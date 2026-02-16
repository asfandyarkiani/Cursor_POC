namespace OracleFusionHCM.LeaveManagement.DTO.DownstreamDTOs
{
    /// <summary>
    /// Oracle Fusion HCM API Response DTO
    /// Maps to Oracle Fusion Leave Response JSON Profile (316175c7-0e45-4869-9ac6-5f9d69882a62)
    /// Contains 70+ fields from Oracle Fusion, but we only need key fields for response mapping
    /// </summary>
    public class CreateLeaveApiResDTO
    {
        public long? PersonAbsenceEntryId { get; set; }
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
