namespace FacilitiesMgmtSystem.DTO.GetInstructionSetsDTO;

/// <summary>
/// Response DTO for Get Instruction Sets API operation.
/// </summary>
public class GetInstructionSetsResponseDTO : BaseResponseDTO
{
    /// <summary>
    /// List of instruction sets.
    /// </summary>
    public List<InstructionSetData>? InstructionSets { get; set; }
}

/// <summary>
/// Instruction set data.
/// </summary>
public class InstructionSetData
{
    /// <summary>
    /// Unique identifier of the instruction set.
    /// </summary>
    public string? InstructionSetId { get; set; }

    /// <summary>
    /// Instruction set name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Instruction set description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category ID this instruction set belongs to.
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Category name.
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Asset type ID this instruction set applies to.
    /// </summary>
    public string? AssetTypeId { get; set; }

    /// <summary>
    /// Asset type name.
    /// </summary>
    public string? AssetTypeName { get; set; }

    /// <summary>
    /// Version number of the instruction set.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Status of the instruction set.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Estimated duration in minutes.
    /// </summary>
    public int? EstimatedDurationMinutes { get; set; }

    /// <summary>
    /// Required skill level.
    /// </summary>
    public string? RequiredSkillLevel { get; set; }

    /// <summary>
    /// List of instruction steps.
    /// </summary>
    public List<InstructionStepData>? Steps { get; set; }
}

/// <summary>
/// Instruction step data.
/// </summary>
public class InstructionStepData
{
    /// <summary>
    /// Step number/sequence.
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Step title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Step description/instructions.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Estimated duration for this step in minutes.
    /// </summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// Safety notes for this step.
    /// </summary>
    public string? SafetyNotes { get; set; }

    /// <summary>
    /// Required tools for this step.
    /// </summary>
    public List<string>? RequiredTools { get; set; }

    /// <summary>
    /// Whether this step is mandatory.
    /// </summary>
    public bool IsMandatory { get; set; } = true;
}
