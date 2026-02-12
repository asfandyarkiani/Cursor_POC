namespace FacilitiesMgmtSystem.DTO.GetBreakdownTaskDTO;

/// <summary>
/// Response DTO for Get Breakdown Task API operation.
/// </summary>
public class GetBreakdownTaskResponseDTO : BaseResponseDTO
{
    /// <summary>
    /// List of breakdown tasks.
    /// </summary>
    public List<BreakdownTaskData>? Tasks { get; set; }
}

/// <summary>
/// Breakdown task data.
/// </summary>
public class BreakdownTaskData
{
    /// <summary>
    /// Unique identifier of the task.
    /// </summary>
    public string? TaskId { get; set; }

    /// <summary>
    /// Task name or title.
    /// </summary>
    public string? TaskName { get; set; }

    /// <summary>
    /// Task description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Associated work order ID.
    /// </summary>
    public string? WorkOrderId { get; set; }

    /// <summary>
    /// Task status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Task priority.
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Estimated duration in hours.
    /// </summary>
    public decimal? EstimatedHours { get; set; }

    /// <summary>
    /// Actual hours worked.
    /// </summary>
    public decimal? ActualHours { get; set; }

    /// <summary>
    /// Assigned technician or team.
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Scheduled start date.
    /// </summary>
    public string? ScheduledStartDate { get; set; }

    /// <summary>
    /// Scheduled end date.
    /// </summary>
    public string? ScheduledEndDate { get; set; }

    /// <summary>
    /// Task sequence number within the work order.
    /// </summary>
    public int? SequenceNumber { get; set; }

    /// <summary>
    /// Instruction set ID associated with this task.
    /// </summary>
    public string? InstructionSetId { get; set; }
}
