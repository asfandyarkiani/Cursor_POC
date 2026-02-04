namespace AGI.SystemLayer.CAFM.DTOs.Downstream;

/// <summary>
/// Request DTO for CreateEvent/Link task SOAP operation
/// </summary>
public class CreateEventRequestDTO
{
    public string SessionId { get; set; } = string.Empty;
    public EventDetails EventDetails { get; set; } = new();
}

public class EventDetails
{
    public string? TaskId { get; set; }
    public string? EventType { get; set; }
    public string? EventDescription { get; set; }
    public string? LinkedTaskId { get; set; }
}

/// <summary>
/// Response DTO for CreateEvent SOAP operation
/// </summary>
public class CreateEventResponseDTO
{
    public string? EventId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
