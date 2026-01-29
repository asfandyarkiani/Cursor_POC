namespace AGI.SystemLayer.CAFM.DTOs.Downstream;

/// <summary>
/// Request DTO for GetInstructionSetsByDto SOAP operation
/// </summary>
public class GetInstructionSetsByDtoRequestDTO
{
    public string SessionId { get; set; } = string.Empty;
    public string? InstructionSetCode { get; set; }
}

/// <summary>
/// Response DTO for GetInstructionSetsByDto SOAP operation
/// </summary>
public class GetInstructionSetsByDtoResponseDTO
{
    public List<InstructionSetData>? InstructionSets { get; set; }
}

public class InstructionSetData
{
    public string? InstructionSetId { get; set; }
    public string? InstructionSetCode { get; set; }
    public string? InstructionSetName { get; set; }
    public string? Description { get; set; }
}
