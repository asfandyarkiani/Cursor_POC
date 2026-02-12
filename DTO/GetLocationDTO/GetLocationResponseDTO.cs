namespace FacilitiesMgmtSystem.DTO.GetLocationDTO;

/// <summary>
/// Response DTO for Get Location API operation.
/// </summary>
public class GetLocationResponseDTO : BaseResponseDTO
{
    /// <summary>
    /// List of locations matching the request criteria.
    /// </summary>
    public List<LocationData>? Locations { get; set; }
}

/// <summary>
/// Location data.
/// </summary>
public class LocationData
{
    /// <summary>
    /// Unique identifier of the location.
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// Location code.
    /// </summary>
    public string? LocationCode { get; set; }

    /// <summary>
    /// Location name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Location description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Location type (e.g., "Room", "Floor", "Building").
    /// </summary>
    public string? LocationType { get; set; }

    /// <summary>
    /// Building ID this location belongs to.
    /// </summary>
    public string? BuildingId { get; set; }

    /// <summary>
    /// Building name.
    /// </summary>
    public string? BuildingName { get; set; }

    /// <summary>
    /// Floor ID this location is on.
    /// </summary>
    public string? FloorId { get; set; }

    /// <summary>
    /// Floor name.
    /// </summary>
    public string? FloorName { get; set; }

    /// <summary>
    /// Parent location ID in hierarchy.
    /// </summary>
    public string? ParentLocationId { get; set; }

    /// <summary>
    /// Full path in location hierarchy.
    /// </summary>
    public string? HierarchyPath { get; set; }

    /// <summary>
    /// Area in square meters.
    /// </summary>
    public decimal? Area { get; set; }

    /// <summary>
    /// Capacity (e.g., number of people).
    /// </summary>
    public int? Capacity { get; set; }

    /// <summary>
    /// Status of the location.
    /// </summary>
    public string? Status { get; set; }
}
