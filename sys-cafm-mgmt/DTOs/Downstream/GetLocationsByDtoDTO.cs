namespace AGI.SystemLayer.CAFM.DTOs.Downstream;

/// <summary>
/// Request DTO for GetLocationsByDto SOAP operation
/// </summary>
public class GetLocationsByDtoRequestDTO
{
    public string SessionId { get; set; } = string.Empty;
    public string BarCode { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for GetLocationsByDto SOAP operation
/// </summary>
public class GetLocationsByDtoResponseDTO
{
    public List<LocationData>? Locations { get; set; }
}

public class LocationData
{
    public string? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? LocationName { get; set; }
    public string? BarCode { get; set; }
    public string? BuildingId { get; set; }
    public string? FloorId { get; set; }
    public string? RoomId { get; set; }
}
