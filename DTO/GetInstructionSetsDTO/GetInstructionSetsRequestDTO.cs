namespace FacilitiesMgmtSystem.DTO.GetInstructionSetsDTO;

/// <summary>
/// Request DTO for Get Instruction Sets API operation.
/// </summary>
public class GetInstructionSetsRequestDTO : BaseRequestDTO
{
    /// <summary>
    /// ID of a specific instruction set to retrieve.
    /// </summary>
    public string? InstructionSetId { get; set; }

    /// <summary>
    /// Category ID to filter instruction sets.
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Asset type ID to filter instruction sets.
    /// </summary>
    public string? AssetTypeId { get; set; }

    /// <summary>
    /// Whether to include detailed steps in the response.
    /// </summary>
    public bool IncludeSteps { get; set; } = true;

    /// <inheritdoc/>
    public override void ValidateAPIRequestParameters()
    {
        base.ValidateAPIRequestParameters();
        // At least one filter must be provided
        if (string.IsNullOrWhiteSpace(InstructionSetId) && 
            string.IsNullOrWhiteSpace(CategoryId) && 
            string.IsNullOrWhiteSpace(AssetTypeId))
        {
            ValidateRequired(InstructionSetId, "InstructionSetId, CategoryId, or AssetTypeId");
        }
    }
}
