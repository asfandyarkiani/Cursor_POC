namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Response DTO for creating an event in CAFM.
/// </summary>
public class CreateEventResponseDto
{
    /// <summary>
    /// Indicates whether the event was created successfully.
    /// </summary>
    public bool IsCreated { get; set; }

    /// <summary>
    /// The Task ID the event was linked to.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;
}
