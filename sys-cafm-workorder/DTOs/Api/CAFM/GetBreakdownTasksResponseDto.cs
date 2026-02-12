namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Response DTO for getting breakdown tasks from CAFM.
/// </summary>
public class GetBreakdownTasksResponseDto
{
    /// <summary>
    /// List of breakdown tasks retrieved from CAFM.
    /// </summary>
    public List<BreakdownTaskDto> Tasks { get; set; } = new();

    /// <summary>
    /// Total count of tasks returned.
    /// </summary>
    public int TotalCount => Tasks.Count;

    /// <summary>
    /// Indicates if any tasks were found.
    /// </summary>
    public bool HasTasks => Tasks.Count > 0;
}

/// <summary>
/// Individual breakdown task from CAFM.
/// </summary>
public class BreakdownTaskDto
{
    public string TaskId { get; set; } = string.Empty;
    public string CallId { get; set; } = string.Empty;
    public string BuildingId { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string InstructionId { get; set; } = string.Empty;
    public string DisciplineId { get; set; } = string.Empty;
    public string PriorityId { get; set; } = string.Empty;
    public string ContractId { get; set; } = string.Empty;
    public string CallerSourceId { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? RaisedDateUtc { get; set; }
    public DateTime? ScheduledDateUtc { get; set; }
}
