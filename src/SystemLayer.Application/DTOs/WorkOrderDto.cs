namespace SystemLayer.Application.DTOs;

public class CreateWorkOrderRequestDto
{
    public string? WorkOrderNumber { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? LocationId { get; set; }
    public string? AssetId { get; set; }
    public string? RequestedBy { get; set; }
    public DateTime? RequestedDate { get; set; }
    public string? WorkType { get; set; }
    public string? Status { get; set; }
    public List<string>? InstructionSetIds { get; set; }
    public string? BreakdownTaskId { get; set; }
}

public class CreateWorkOrderResponseDto
{
    public bool Success { get; set; }
    public string? WorkOrderId { get; set; }
    public string? WorkOrderNumber { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

public class GetBreakdownTaskRequestDto
{
    public string? TaskId { get; set; }
    public string? AssetId { get; set; }
    public string? LocationId { get; set; }
}

public class GetBreakdownTaskResponseDto
{
    public bool Success { get; set; }
    public string? TaskId { get; set; }
    public string? TaskName { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? EstimatedHours { get; set; }
    public List<string>? RequiredSkills { get; set; }
    public List<string>? Errors { get; set; }
}

public class GetLocationRequestDto
{
    public string? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? BuildingId { get; set; }
}

public class GetLocationResponseDto
{
    public bool Success { get; set; }
    public string? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? LocationName { get; set; }
    public string? BuildingId { get; set; }
    public string? BuildingName { get; set; }
    public string? FloorId { get; set; }
    public string? FloorName { get; set; }
    public List<string>? Errors { get; set; }
}

public class GetInstructionSetsRequestDto
{
    public string? AssetId { get; set; }
    public string? WorkType { get; set; }
    public string? LocationId { get; set; }
}

public class GetInstructionSetsResponseDto
{
    public bool Success { get; set; }
    public List<InstructionSetDto>? InstructionSets { get; set; }
    public List<string>? Errors { get; set; }
}

public class InstructionSetDto
{
    public string? InstructionSetId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? WorkType { get; set; }
    public List<InstructionStepDto>? Steps { get; set; }
}

public class InstructionStepDto
{
    public string? StepId { get; set; }
    public int? Sequence { get; set; }
    public string? Description { get; set; }
    public string? EstimatedMinutes { get; set; }
}