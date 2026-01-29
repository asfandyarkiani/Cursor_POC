namespace AGI.SystemLayer.CAFM.DTOs.Downstream;

/// <summary>
/// Request DTO for GetBreakdownTasksByDto SOAP operation
/// </summary>
public class GetBreakdownTasksByDtoRequestDTO
{
    public string SessionId { get; set; } = string.Empty;
    public string? TaskId { get; set; }
    public string? TaskCode { get; set; }
}

/// <summary>
/// Response DTO for GetBreakdownTasksByDto SOAP operation
/// </summary>
public class GetBreakdownTasksByDtoResponseDTO
{
    public List<BreakdownTaskData>? BreakdownTasks { get; set; }
}

public class BreakdownTaskData
{
    public string? TaskId { get; set; }
    public string? TaskCode { get; set; }
    public string? TaskName { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
}
