using System.ComponentModel.DataAnnotations;

namespace SystemLayer.Application.DTOs;

/// <summary>
/// Request DTOs received from Process Layer
/// </summary>
public class CreateWorkOrderRequest
{
    [Required]
    public string WorkOrderNumber { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string LocationId { get; set; } = string.Empty;
    
    public string? Priority { get; set; }
    
    public string? AssignedTo { get; set; }
    
    public DateTime? ScheduledDate { get; set; }
    
    public string? InstructionSetId { get; set; }
    
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
}

public class GetLocationRequest
{
    [Required]
    public string LocationId { get; set; } = string.Empty;
    
    public bool IncludeHierarchy { get; set; } = false;
}

public class GetBreakdownTaskRequest
{
    [Required]
    public string TaskId { get; set; } = string.Empty;
    
    public bool IncludeDetails { get; set; } = true;
}

public class GetInstructionSetsRequest
{
    public string? CategoryFilter { get; set; }
    
    public string? AssetTypeFilter { get; set; }
    
    public int MaxResults { get; set; } = 100;
}

/// <summary>
/// Response DTOs returned to Process Layer
/// </summary>
public class CreateWorkOrderResponse
{
    public bool Success { get; set; }
    
    public string? WorkOrderId { get; set; }
    
    public string? WorkOrderNumber { get; set; }
    
    public string? Message { get; set; }
    
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class LocationResponse
{
    public string LocationId { get; set; } = string.Empty;
    
    public string LocationName { get; set; } = string.Empty;
    
    public string? ParentLocationId { get; set; }
    
    public string? LocationType { get; set; }
    
    public string? Status { get; set; }
    
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class BreakdownTaskResponse
{
    public string TaskId { get; set; } = string.Empty;
    
    public string TaskName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? Status { get; set; }
    
    public TimeSpan? EstimatedDuration { get; set; }
    
    public List<string> RequiredSkills { get; set; } = new();
    
    public Dictionary<string, object> TaskDetails { get; set; } = new();
}

public class InstructionSetResponse
{
    public string InstructionSetId { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string? Category { get; set; }
    
    public string? AssetType { get; set; }
    
    public List<InstructionStep> Steps { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class InstructionStep
{
    public int StepNumber { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public TimeSpan? EstimatedDuration { get; set; }
    
    public List<string> RequiredTools { get; set; } = new();
    
    public Dictionary<string, object> StepData { get; set; } = new();
}

public class GetInstructionSetsResponse
{
    public List<InstructionSetResponse> InstructionSets { get; set; } = new();
    
    public int TotalCount { get; set; }
    
    public bool HasMore { get; set; }
}