namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Response DTO for getting locations from CAFM.
/// </summary>
public class GetLocationsResponseDto
{
    /// <summary>
    /// List of locations retrieved from CAFM.
    /// </summary>
    public List<LocationDto> Locations { get; set; } = new();

    /// <summary>
    /// Total count of locations returned.
    /// </summary>
    public int TotalCount => Locations.Count;

    /// <summary>
    /// Indicates if any locations were found.
    /// </summary>
    public bool HasLocations => Locations.Count > 0;
}

/// <summary>
/// Individual location from CAFM.
/// </summary>
public class LocationDto
{
    public string LocationId { get; set; } = string.Empty;
    public string BuildingId { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
}
