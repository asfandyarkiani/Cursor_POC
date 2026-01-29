namespace AGI.SystemLayer.CAFM.DTOs.Downstream;

/// <summary>
/// Request DTO for CreateBreakdownTask SOAP operation
/// </summary>
public class CreateBreakdownTaskRequestDTO
{
    public string SessionId { get; set; } = string.Empty;
    public BreakdownTaskDetails TaskDetails { get; set; } = new();
}

public class BreakdownTaskDetails
{
    public string? LocationId { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? RequestedBy { get; set; }
    public string? ContactPhone { get; set; }
    public string? Notes { get; set; }
    public string? InstructionSetId { get; set; }
}

/// <summary>
/// Response DTO for CreateBreakdownTask SOAP operation
/// </summary>
public class CreateBreakdownTaskResponseDTO
{
    public string? TaskId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
