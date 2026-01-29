namespace FacilitiesMgmtSystem.DTO.GetLocationDTO;

/// <summary>
/// Request DTO for Get Location API operation.
/// </summary>
public class GetLocationRequestDTO : BaseRequestDTO
{
    /// <summary>
    /// ID of the location to retrieve.
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// Location code for lookup.
    /// </summary>
    public string? LocationCode { get; set; }

    /// <summary>
    /// Building ID to filter locations.
    /// </summary>
    public string? BuildingId { get; set; }

    /// <summary>
    /// Floor ID to filter locations.
    /// </summary>
    public string? FloorId { get; set; }

    /// <summary>
    /// Whether to include location hierarchy information.
    /// </summary>
    public bool IncludeHierarchy { get; set; } = false;

    /// <inheritdoc/>
    public override void ValidateAPIRequestParameters()
    {
        base.ValidateAPIRequestParameters();
        // At least one identifier must be provided
        if (string.IsNullOrWhiteSpace(LocationId) && 
            string.IsNullOrWhiteSpace(LocationCode) && 
            string.IsNullOrWhiteSpace(BuildingId))
        {
            ValidateRequired(LocationId, "LocationId, LocationCode, or BuildingId");
        }
    }
}
