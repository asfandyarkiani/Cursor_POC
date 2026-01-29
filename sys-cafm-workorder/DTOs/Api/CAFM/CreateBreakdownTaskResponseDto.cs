namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Response DTO for creating a breakdown task in CAFM.
/// </summary>
public class CreateBreakdownTaskResponseDto
{
    /// <summary>
    /// The Task ID created in CAFM.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the task was created successfully.
    /// </summary>
    public bool IsCreated { get; set; }

    /// <summary>
    /// The caller source ID (service request number) associated with this task.
    /// </summary>
    public string CallerSourceId { get; set; } = string.Empty;
}
