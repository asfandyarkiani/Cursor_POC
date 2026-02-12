namespace FacilitiesMgmtSystem.DTO.GetBreakdownTaskDTO;

/// <summary>
/// Request DTO for Get Breakdown Task API operation.
/// </summary>
public class GetBreakdownTaskRequestDTO : BaseRequestDTO
{
    /// <summary>
    /// ID of the task to retrieve.
    /// </summary>
    public string? TaskId { get; set; }

    /// <summary>
    /// ID of the work order to get tasks for.
    /// </summary>
    public string? WorkOrderId { get; set; }

    /// <summary>
    /// Whether to include detailed task information.
    /// </summary>
    public bool IncludeDetails { get; set; } = true;

    /// <inheritdoc/>
    public override void ValidateAPIRequestParameters()
    {
        base.ValidateAPIRequestParameters();
        // Either TaskId or WorkOrderId must be provided
        if (string.IsNullOrWhiteSpace(TaskId) && string.IsNullOrWhiteSpace(WorkOrderId))
        {
            ValidateRequired(TaskId, "TaskId or WorkOrderId");
        }
    }
}
