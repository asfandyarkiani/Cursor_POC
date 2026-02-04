using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Request DTO for creating a breakdown task in CAFM.
/// </summary>
public class CreateBreakdownTaskRequestDto : IRequestSysDTO
{
    /// <summary>
    /// CAFM Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Building ID where the task is located.
    /// </summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>
    /// Location ID within the building.
    /// </summary>
    public string LocationId { get; set; } = string.Empty;

    /// <summary>
    /// Category ID for the task type.
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// Instruction ID for the task.
    /// </summary>
    public string InstructionId { get; set; } = string.Empty;

    /// <summary>
    /// Discipline/Labor ID.
    /// </summary>
    public string DisciplineId { get; set; } = string.Empty;

    /// <summary>
    /// Priority ID for the task.
    /// </summary>
    public string PriorityId { get; set; } = string.Empty;

    /// <summary>
    /// Contract ID. If not provided, uses configured default.
    /// </summary>
    public string? ContractId { get; set; }

    /// <summary>
    /// Caller source ID (e.g., service request number from EQ+).
    /// </summary>
    public string CallerSourceId { get; set; } = string.Empty;

    /// <summary>
    /// Reporter's name.
    /// </summary>
    public string ReporterName { get; set; } = string.Empty;

    /// <summary>
    /// Reporter's email address.
    /// </summary>
    public string ReporterEmail { get; set; } = string.Empty;

    /// <summary>
    /// Reporter's phone number.
    /// </summary>
    public string ReporterPhone { get; set; } = string.Empty;

    /// <summary>
    /// Description of the task/issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date when the task was raised (UTC).
    /// </summary>
    public DateTime? RaisedDateUtc { get; set; }

    /// <summary>
    /// Scheduled date for the task (UTC).
    /// </summary>
    public DateTime? ScheduledDateUtc { get; set; }

    /// <summary>
    /// Technician name assigned to the task.
    /// </summary>
    public string? Technician { get; set; }

    /// <summary>
    /// Source organization ID.
    /// </summary>
    public string? SourceOrgId { get; set; }

    public void ValidateAPIRequestParameters()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SessionId))
            errors.Add("SessionId is required.");

        if (string.IsNullOrWhiteSpace(BuildingId))
            errors.Add("BuildingId is required.");

        if (string.IsNullOrWhiteSpace(LocationId))
            errors.Add("LocationId is required.");

        if (string.IsNullOrWhiteSpace(CategoryId))
            errors.Add("CategoryId is required.");

        if (string.IsNullOrWhiteSpace(InstructionId))
            errors.Add("InstructionId is required.");

        if (string.IsNullOrWhiteSpace(DisciplineId))
            errors.Add("DisciplineId is required.");

        if (string.IsNullOrWhiteSpace(PriorityId))
            errors.Add("PriorityId is required.");

        if (string.IsNullOrWhiteSpace(CallerSourceId))
            errors.Add("CallerSourceId is required.");

        if (string.IsNullOrWhiteSpace(ReporterName))
            errors.Add("ReporterName is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(CreateBreakdownTaskRequestDto));
        }
    }
}
