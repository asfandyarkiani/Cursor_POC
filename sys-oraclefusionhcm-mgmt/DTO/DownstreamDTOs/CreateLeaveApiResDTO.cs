namespace OracleFusionHCMSystem.DTO.DownstreamDTOs
{
    public class CreateLeaveApiResDTO
    {
        public long? PersonAbsenceEntryId { get; set; }
        public string? AbsenceCaseId { get; set; }
        public bool? AbsenceEntryBasicFlag { get; set; }
        public string? AbsencePatternCd { get; set; }
        public string? AbsenceStatusCd { get; set; }
        public long? AbsenceTypeId { get; set; }
        public string? AbsenceTypeReasonId { get; set; }
        public string? AgreementId { get; set; }
        public string? ApprovalStatusCd { get; set; }
        public string? AuthStatusUpdateDate { get; set; }
        public string? BandDtlId { get; set; }
        public string? BlockedLeaveCandidate { get; set; }
        public string? CertificationAuthFlag { get; set; }
        public string? ChildEventTypeCd { get; set; }
        public string? Comments { get; set; }
        public string? ConditionStartDate { get; set; }
        public string? ConfirmedDate { get; set; }
        public string? ConsumedByAgreement { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreationDate { get; set; }
        public string? DiseaseCode { get; set; }
        public int? Duration { get; set; }
        public bool? EmployeeShiftFlag { get; set; }
        public string? EndDate { get; set; }
        public int? EndDateDuration { get; set; }
        public string? EndDateTime { get; set; }
        public string? EndTime { get; set; }
        public string? EstablishmentDate { get; set; }
        public string? Frequency { get; set; }
        public string? InitialReportById { get; set; }
        public string? InitialTimelyNotifyFlag { get; set; }
        public string? LastUpdateDate { get; set; }
        public string? LastUpdateLogin { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? LateNotifyFlag { get; set; }
        public long? LegalEntityId { get; set; }
        public string? LegislationCode { get; set; }
        public long? LegislativeDataGroupId { get; set; }
        public string? NotificationDate { get; set; }
        public long? ObjectVersionNumber { get; set; }
        public bool? OpenEndedFlag { get; set; }
        public string? Overridden { get; set; }
        public string? PeriodOfIncapToWorkFlag { get; set; }
        public long? PeriodOfServiceId { get; set; }
        public long? PersonId { get; set; }
        public string? PlannedEndDate { get; set; }
        public string? ProcessingStatus { get; set; }
        public string? ProjectId { get; set; }
        public bool? SingleDayFlag { get; set; }
        public string? Source { get; set; }
        public string? SplCondition { get; set; }
        public string? StartDate { get; set; }
        public int? StartDateDuration { get; set; }
        public string? StartDateTime { get; set; }
        public string? StartTime { get; set; }
        public string? SubmittedDate { get; set; }
        public string? TimelinessOverrideDate { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? UserMode { get; set; }
        public string? PersonNumber { get; set; }
        public string? AbsenceType { get; set; }
        public string? Employer { get; set; }
        public string? AbsenceReason { get; set; }
        public string? AbsenceDispStatus { get; set; }
        public string? AssignmentId { get; set; }
        public long? DataSecurityPersonId { get; set; }
        public string? EffectiveEndDate { get; set; }
        public string? EffectiveStartDate { get; set; }
        public string? AgreementName { get; set; }
        public string? PaymentDetail { get; set; }
        public string? AssignmentName { get; set; }
        public string? AssignmentNumber { get; set; }
        public string? UnitOfMeasureMeaning { get; set; }
        public string? FormattedDuration { get; set; }
        public string? AbsenceDispStatusMeaning { get; set; }
        public string? AbsenceUpdatableFlag { get; set; }
        public string? ApprovalDatetime { get; set; }
        public bool? AllowAssignmentSelectionFlag { get; set; }
        public List<LinkItem>? Links { get; set; }
    }

    public class LinkItem
    {
        public string? Rel { get; set; }
        public string? Href { get; set; }
        public string? Name { get; set; }
        public string? Kind { get; set; }
        public LinkProperties? Properties { get; set; }
    }

    public class LinkProperties
    {
        public string? ChangeIndicator { get; set; }
    }
}
