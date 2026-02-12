namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Response DTO for getting instruction sets from CAFM.
/// </summary>
public class GetInstructionSetsResponseDto
{
    /// <summary>
    /// List of instruction sets retrieved from CAFM.
    /// </summary>
    public List<InstructionSetDto> InstructionSets { get; set; } = new();

    /// <summary>
    /// Total count of instruction sets returned.
    /// </summary>
    public int TotalCount => InstructionSets.Count;

    /// <summary>
    /// Indicates if any instruction sets were found.
    /// </summary>
    public bool HasInstructionSets => InstructionSets.Count > 0;
}

/// <summary>
/// Individual instruction set from CAFM.
/// </summary>
public class InstructionSetDto
{
    /// <summary>
    /// Instruction ID (IN_SEQ).
    /// </summary>
    public string InstructionId { get; set; } = string.Empty;

    /// <summary>
    /// Category ID (IN_FKEY_CAT_SEQ).
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// Discipline/Labor ID (IN_FKEY_LAB_SEQ).
    /// </summary>
    public string DisciplineId { get; set; } = string.Empty;

    /// <summary>
    /// Priority ID (IN_FKEY_PRI_SEQ).
    /// </summary>
    public string PriorityId { get; set; } = string.Empty;

    /// <summary>
    /// Instruction name/description.
    /// </summary>
    public string InstructionName { get; set; } = string.Empty;
}
